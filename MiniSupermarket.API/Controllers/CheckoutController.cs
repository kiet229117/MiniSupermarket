/*
 * Ho ten sinh vien: Nguyen Van Teo
 * Ma sinh vien: 123456789
 * Ngay tao: 15/09/2026
 * Mo ta: API Thanh toan POS da bang duoc bao ve boi SqlTransaction (ACID) - Async/Await 100%
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MiniSupermarket.API.Models;

namespace MiniSupermarket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckoutController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        // Tiêm IConfiguration de doc chuoi ket noi CSDL
        public CheckoutController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // =========================================================================
        // POST: api/checkout
        // Chuc nang: Thanh toan hoa don, tru ton kho va tich diem trong 1 Transaction (Bat dong bo)
        // =========================================================================
        [HttpPost]
        public async Task<IActionResult> ProcessCheckout([FromBody] CheckoutRequestDto request)
        {
            // 1. Kiem tra tinh hop le cua gio hang
            if (request.Items == null || request.Items.Count == 0)
            {
                return BadRequest(new { message = "Giỏ hàng rỗng, không thể tiến hành thanh toán!" });
            }

            if (string.IsNullOrWhiteSpace(request.CashierUsername))
            {
                return BadRequest(new { message = "Thiếu thông tin tài khoản thu ngân lập hóa đơn!" });
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection")!;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // 2. KHOI TAO TRANSACTION (Bat dau phien giao dich khep kin bao ve ACID)
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Tinh toan tong tien, tong so luong va diem thuong tich luy
                    decimal finalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice);
                    int totalItems = request.Items.Sum(i => i.Quantity);

                    // Quy tac tich diem sieu thi: Cu moi 10.000d mua hang duoc tang 1 diem thuong
                    int rewardPoints = (int)(finalAmount / 10000);

                    // Sinh ma hoa don theo cau truc: HD-YYYYMMDD-XXXX (4 ky tu ngau nhien)
                    string orderCode = $"HD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

                    // =====================================================================
                    // BƯỚC 1: TAO HOA DON TONG THE TRONG BANG Orders
                    // =====================================================================
                    // Su dung OUTPUT INSERTED.OrderId de lay ve gia tri khoa chinh vua tu dong tang
                    string insertOrderSql = @"
                        INSERT INTO Orders (OrderCode, CreatedDate, CashierUsername, CustomerId, TotalItems, FinalAmount, RewardPoints)
                        OUTPUT INSERTED.OrderId
                        VALUES (@OrderCode, GETDATE(), @CashierUsername, @CustomerId, @TotalItems, @FinalAmount, @RewardPoints)";

                    int orderId = 0;
                    using (SqlCommand cmdOrder = new SqlCommand(insertOrderSql, connection, transaction))
                    {
                        // Gan tham so an toan
                        cmdOrder.Parameters.AddWithValue("@OrderCode", orderCode);
                        cmdOrder.Parameters.AddWithValue("@CashierUsername", request.CashierUsername.Trim());
                        cmdOrder.Parameters.AddWithValue("@CustomerId", request.CustomerId);
                        cmdOrder.Parameters.AddWithValue("@TotalItems", totalItems);
                        cmdOrder.Parameters.AddWithValue("@FinalAmount", finalAmount);
                        cmdOrder.Parameters.AddWithValue("@RewardPoints", rewardPoints);

                        // ExecuteScalarAsync() tra ve gia tri OrderId vua duoc he thong sinh ra
                        var scalarResult = await cmdOrder.ExecuteScalarAsync();
                        orderId = Convert.ToInt32(scalarResult);
                    }

                    // =====================================================================
                    // BƯỚC 2 & 3: XU LY TUNG MON HANG (KIEM TRA, LUU CHI TIET VA TRU TON KHO)
                    // =====================================================================
                    foreach (var item in request.Items)
                    {
                        // 3.1. Kiem tra so luong ton kho thuc te truoc khi tru
                        string checkStockSql = "SELECT StockQuantity, ProductName FROM Products WHERE ProductId = @ProductId";
                        int currentStock = 0;
                        string productName = string.Empty;

                        using (SqlCommand cmdCheck = new SqlCommand(checkStockSql, connection, transaction))
                        {
                            cmdCheck.Parameters.AddWithValue("@ProductId", item.ProductId);

                            using (SqlDataReader reader = await cmdCheck.ExecuteReaderAsync())
                            {
                                if (!await reader.ReadAsync())
                                {
                                    throw new InvalidOperationException($"Không tìm thấy sản phẩm có ID: {item.ProductId}");
                                }
                                currentStock = Convert.ToInt32(reader["StockQuantity"]);
                                productName = reader["ProductName"].ToString() ?? string.Empty;
                            }
                        }

                        // 3.2. DIEM KIEM SOAT LOGIC: Neu so luong mua vuot qua ton kho -> Kich hoat Exception
                        if (currentStock < item.Quantity)
                        {
                            throw new InvalidOperationException(
                                $"Sản phẩm '{productName}' (ID: {item.ProductId}) không đủ tồn kho! " +
                                $"(Tồn kho thực tế: {currentStock}, Số lượng yêu cầu mua: {item.Quantity})");
                        }

                        // 3.3. Them dong chi tiet vao bang OrderDetails
                        string insertDetailSql = @"
                            INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice, LineTotal)
                            VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @LineTotal)";

                        using (SqlCommand cmdDetail = new SqlCommand(insertDetailSql, connection, transaction))
                        {
                            cmdDetail.Parameters.AddWithValue("@OrderId", orderId);
                            cmdDetail.Parameters.AddWithValue("@ProductId", item.ProductId);
                            cmdDetail.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmdDetail.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                            cmdDetail.Parameters.AddWithValue("@LineTotal", item.Quantity * item.UnitPrice);
                            await cmdDetail.ExecuteNonQueryAsync();
                        }

                        // 3.4. Khau tru so luong ton kho trong bang Products
                        string updateStockSql = @"
                            UPDATE Products 
                            SET StockQuantity = StockQuantity - @Qty 
                            WHERE ProductId = @ProductId";

                        using (SqlCommand cmdStock = new SqlCommand(updateStockSql, connection, transaction))
                        {
                            cmdStock.Parameters.AddWithValue("@Qty", item.Quantity);
                            cmdStock.Parameters.AddWithValue("@ProductId", item.ProductId);
                            await cmdStock.ExecuteNonQueryAsync();
                        }
                    }

                    // =====================================================================
                    // BƯỚC 4: CONG DIEM TICH LUY CHO KHACH HANG TRONG BANG Customers
                    // =====================================================================
                    if (rewardPoints > 0)
                    {
                        string updatePointsSql = @"
                            UPDATE Customers 
                            SET RewardPoints = RewardPoints + @Points 
                            WHERE CustomerId = @CustomerId";

                        using (SqlCommand cmdCustomer = new SqlCommand(updatePointsSql, connection, transaction))
                        {
                            cmdCustomer.Parameters.AddWithValue("@Points", rewardPoints);
                            cmdCustomer.Parameters.AddWithValue("@CustomerId", request.CustomerId);
                            await cmdCustomer.ExecuteNonQueryAsync();
                        }
                    }

                    // =====================================================================
                    // BƯỚC 5: XAC NHAN LUU VINH VIEN TOAN BO THAO TAC (COMMIT)
                    // =====================================================================
                    await transaction.CommitAsync();

                    // Tra ve ket qua thanh cong voi ma 200 OK
                    return Ok(new
                    {
                        success = true,
                        message = "Thanh toán hóa đơn POS thành công!",
                        orderId = orderId,
                        orderCode = orderCode,
                        totalItems = totalItems,
                        finalAmount = finalAmount,
                        rewardPointsAdded = rewardPoints
                    });
                }
                catch (Exception ex)
                {
                    // =====================================================================
                    // BƯỚC 6: HOAN TAC TOAN BO DU LIEU SACH SE KHI CO BAT KY LOI NAO (ROLLBACK)
                    // =====================================================================
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch (Exception rollbackEx)
                    {
                        return StatusCode(500, new 
                        { 
                            success = false,
                            message = "Lỗi nghiêm trọng khi thực thi Rollback: " + rollbackEx.Message 
                        });
                    }

                    // Tra ve thong bao loi than thien va ma 400 BadRequest
                    return BadRequest(new
                    {
                        success = false,
                        message = "Giao dịch thanh toán bị hủy và đã được Rollback an toàn!",
                        errorDetail = ex.Message
                    });
                }
            }
        }

        // =========================================================================
        // DELETE: api/checkout/cancel/{orderId}
        // Chuc nang: Huy hoa don, hoan tra ton kho va khau tru diem thuong trong 1 Transaction (Bat dong bo)
        // =========================================================================
        [HttpDelete("cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection")!;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // 1. Kiem tra hoa don va lay thong tin khach hang, diem thuong
                    string getOrderSql = "SELECT CustomerId, RewardPoints FROM Orders WHERE OrderId = @OrderId";
                    int customerId = 0;
                    int pointsToDeduct = 0;

                    using (SqlCommand cmdGet = new SqlCommand(getOrderSql, connection, transaction))
                    {
                        cmdGet.Parameters.AddWithValue("@OrderId", orderId);
                        using (SqlDataReader reader = await cmdGet.ExecuteReaderAsync())
                        {
                            if (!await reader.ReadAsync())
                            {
                                throw new InvalidOperationException($"Không tìm thấy hóa đơn ID = {orderId} để hủy!");
                            }
                            customerId = reader["CustomerId"] != DBNull.Value ? Convert.ToInt32(reader["CustomerId"]) : 0;
                            pointsToDeduct = reader["RewardPoints"] != DBNull.Value ? Convert.ToInt32(reader["RewardPoints"]) : 0;
                        }
                    }

                    // 2. Lay danh sach san pham da mua de hoan tra kho
                    var itemsToRestore = new List<(int ProductId, int Quantity)>();
                    string getDetailsSql = "SELECT ProductId, Quantity FROM OrderDetails WHERE OrderId = @OrderId";

                    using (SqlCommand cmdDetails = new SqlCommand(getDetailsSql, connection, transaction))
                    {
                        cmdDetails.Parameters.AddWithValue("@OrderId", orderId);
                        using (SqlDataReader reader = await cmdDetails.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                itemsToRestore.Add((
                                    Convert.ToInt32(reader["ProductId"]), 
                                    Convert.ToInt32(reader["Quantity"])
                                ));
                            }
                        }
                    }

                    // 3. Hoan tra ton kho trong bang Products
                    foreach (var item in itemsToRestore)
                    {
                        string restoreStockSql = @"
                            UPDATE Products 
                            SET StockQuantity = StockQuantity + @Qty 
                            WHERE ProductId = @ProductId";

                        using (SqlCommand cmdRestore = new SqlCommand(restoreStockSql, connection, transaction))
                        {
                            cmdRestore.Parameters.AddWithValue("@Qty", item.Quantity);
                            cmdRestore.Parameters.AddWithValue("@ProductId", item.ProductId);
                            await cmdRestore.ExecuteNonQueryAsync();
                        }
                    }

                    // 4. Khau tru diem thuong da tich cua khach hang
                    if (pointsToDeduct > 0 && customerId > 0)
                    {
                        string deductPointsSql = @"
                            UPDATE Customers 
                            SET RewardPoints = CASE 
                                WHEN RewardPoints >= @Pts THEN RewardPoints - @Pts 
                                ELSE 0 
                            END 
                            WHERE CustomerId = @CustomerId";

                        using (SqlCommand cmdDeduct = new SqlCommand(deductPointsSql, connection, transaction))
                        {
                            cmdDeduct.Parameters.AddWithValue("@Pts", pointsToDeduct);
                            cmdDeduct.Parameters.AddWithValue("@CustomerId", customerId);
                            await cmdDeduct.ExecuteNonQueryAsync();
                        }
                    }

                    // 5. Xoa chi tiet hoa don truoc (tranh vi pham rang buoc khoa ngoai)
                    string deleteDetailsSql = "DELETE FROM OrderDetails WHERE OrderId = @OrderId";
                    using (SqlCommand cmdDelDetails = new SqlCommand(deleteDetailsSql, connection, transaction))
                    {
                        cmdDelDetails.Parameters.AddWithValue("@OrderId", orderId);
                        await cmdDelDetails.ExecuteNonQueryAsync();
                    }

                    // 6. Xoa hoa don tong trong bang Orders
                    string deleteOrderSql = "DELETE FROM Orders WHERE OrderId = @OrderId";
                    using (SqlCommand cmdDelOrder = new SqlCommand(deleteOrderSql, connection, transaction))
                    {
                        cmdDelOrder.Parameters.AddWithValue("@OrderId", orderId);
                        await cmdDelOrder.ExecuteNonQueryAsync();
                    }

                    // 7. Xac nhan luu vinh vien (Commit)
                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        success = true,
                        message = $"Đã hủy thành công hóa đơn #{orderId} và hoàn trả toàn bộ tồn kho vào CSDL!"
                    });
                }
                catch (Exception ex)
                {
                    // Hoan tac sach se khi co loi
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch { }

                    return BadRequest(new
                    {
                        success = false,
                        message = "Hủy hóa đơn thất bại, giao dịch đã được Rollback!",
                        errorDetail = ex.Message
                    });
                }
            }
        }
    }
}
