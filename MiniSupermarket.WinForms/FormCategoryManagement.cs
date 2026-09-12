using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormCategoryManagement : Form
    {
        // =====================================================
        // DANH SÁCH NHÓM HÀNG
        // =====================================================

        private List<Category> categories =
            new List<Category>();


        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public FormCategoryManagement()
        {
            InitializeComponent();
        }


        // =====================================================
        // FORM LOAD
        // =====================================================

        private void FormCategoryManagement_Load_1(
            object sender,
            EventArgs e)
        {
            LoadData();

            SelectFirstRow();

            txtSearch.Text =
                "Nhập từ khóa...";

            txtSearch.ForeColor =
                Color.Gray;

            lblStatus.Text =
                "Ready";
        }


        // =====================================================
        // DỮ LIỆU MẪU
        // =====================================================

        private void LoadData()
        {
            categories.Clear();

            categories.Add(
                new Category
                {
                    ID = 1,
                    Name = "Bánh kẹo & đồ ăn vặt",
                    Description =
                        "Các loại snack, bánh quy, kẹo dẻo, sô-cô-la..."
                });

            categories.Add(
                new Category
                {
                    ID = 2,
                    Name = "Nước giải khát & Trà",
                    Description =
                        "Nước ngọt, nước khoáng..."
                });

            categories.Add(
                new Category
                {
                    ID = 3,
                    Name = "Sữa & Sản phẩm từ sữa",
                    Description =
                        "Sữa tươi, sữa chua..."
                });

            categories.Add(
                new Category
                {
                    ID = 4,
                    Name = "Mì gói & Thực phẩm ăn liền",
                    Description =
                        "Mì ăn liền, phở khô..."
                });

            categories.Add(
                new Category
                {
                    ID = 5,
                    Name = "Gia vị & Dầu ăn",
                    Description =
                        "Nước mắm, hạt nêm..."
                });

            DisplayCategories(categories);
        }


        // =====================================================
        // HIỂN THỊ DANH SÁCH
        // =====================================================

        private void DisplayCategories(
            List<Category> list)
        {
            dataGridView1.Rows.Clear();

            foreach (Category category in list)
            {
                int rowIndex =
                    dataGridView1.Rows.Add();

                DataGridViewRow row =
                    dataGridView1.Rows[rowIndex];

                row.Cells[0].Value =
                    category.ID;

                row.Cells[1].Value =
                    category.Name;

                row.Cells[2].Value =
                    category.Description;
            }

            lblStatus.Text =
                "Có " + list.Count + " nhóm hàng";
        }


        // =====================================================
        // CLICK VÀO DÒNG
        // =====================================================

        private void dataGridView1_CellClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            ShowCategory(e.RowIndex);
        }


        // =====================================================
        // HIỂN THỊ CHI TIẾT
        // =====================================================

        private void ShowCategory(
            int rowIndex)
        {
            if (rowIndex < 0 ||
                rowIndex >= dataGridView1.Rows.Count)
            {
                return;
            }

            DataGridViewRow row =
                dataGridView1.Rows[rowIndex];

            txtID.Text =
                Convert.ToString(
                    row.Cells[0].Value);

            txtName.Text =
                Convert.ToString(
                    row.Cells[1].Value);

            txtDescription.Text =
                Convert.ToString(
                    row.Cells[2].Value);
        }


        // =====================================================
        // CHỌN DÒNG ĐẦU TIÊN
        // =====================================================

        private void SelectFirstRow()
        {
            if (dataGridView1.Rows.Count > 0)
            {
                dataGridView1.ClearSelection();

                dataGridView1.Rows[0].Selected =
                    true;

                dataGridView1.CurrentCell =
                    dataGridView1.Rows[0].Cells[0];

                ShowCategory(0);
            }
            else
            {
                ClearInput();
            }
        }


        // =====================================================
        // XÓA Ô NHẬP
        // =====================================================

        private void ClearInput()
        {
            txtID.Clear();
            txtName.Clear();
            txtDescription.Clear();
        }


        // =====================================================
        // TÌM KIẾM
        // =====================================================

        private void btnSearch_Click(
            object sender,
            EventArgs e)
        {
            string keyword =
                txtSearch.Text.Trim();

            if (string.IsNullOrWhiteSpace(keyword) ||
                keyword == "Nhập từ khóa...")
            {
                DisplayCategories(categories);

                SelectFirstRow();

                return;
            }

            List<Category> result =
                categories
                    .Where(x =>
                        x.Name.IndexOf(
                            keyword,
                            StringComparison.OrdinalIgnoreCase
                        ) >= 0

                        ||

                        x.Description.IndexOf(
                            keyword,
                            StringComparison.OrdinalIgnoreCase
                        ) >= 0
                    )
                    .ToList();

            DisplayCategories(result);

            if (result.Count > 0)
            {
                dataGridView1.ClearSelection();

                dataGridView1.Rows[0].Selected =
                    true;

                dataGridView1.CurrentCell =
                    dataGridView1.Rows[0].Cells[0];

                ShowCategory(0);

                lblStatus.Text =
                    "Tìm thấy " +
                    result.Count +
                    " nhóm hàng";
            }
            else
            {
                ClearInput();

                lblStatus.Text =
                    "Không tìm thấy";

                MessageBox.Show(
                    "Không tìm thấy nhóm hàng!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }


        // =====================================================
        // TẢI LẠI
        // =====================================================

        private void btnReload_Click(
            object sender,
            EventArgs e)
        {
            txtSearch.Text =
                "Nhập từ khóa...";

            txtSearch.ForeColor =
                Color.Gray;

            DisplayCategories(categories);

            SelectFirstRow();

            lblStatus.Text =
                "Đã tải lại dữ liệu";
        }


        // =====================================================
        // THÊM MỚI
        // =====================================================

        private void btnAdd_Click(
            object sender,
            EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                txtName.Text))
            {
                MessageBox.Show(
                    "Vui lòng nhập Tên Nhóm hàng!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtName.Focus();

                return;
            }

            int newID = 1;

            if (categories.Count > 0)
            {
                newID =
                    categories.Max(
                        x => x.ID) + 1;
            }

            Category category =
                new Category
                {
                    ID = newID,

                    Name =
                        txtName.Text.Trim(),

                    Description =
                        txtDescription.Text.Trim()
                };

            categories.Add(category);

            DisplayCategories(categories);

            SelectCategoryByID(newID);

            lblStatus.Text =
                "Đã thêm nhóm hàng";

            MessageBox.Show(
                "Thêm mới thành công!",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }


        // =====================================================
        // CẬP NHẬT
        // =====================================================

        private void btnUpdate_Click(
            object sender,
            EventArgs e)
        {
            int id;

            if (!int.TryParse(
                txtID.Text,
                out id))
            {
                MessageBox.Show(
                    "Vui lòng chọn nhóm hàng cần cập nhật!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            Category category =
                categories.FirstOrDefault(
                    x => x.ID == id);

            if (category == null)
            {
                MessageBox.Show(
                    "Không tìm thấy nhóm hàng!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            if (string.IsNullOrWhiteSpace(
                txtName.Text))
            {
                MessageBox.Show(
                    "Vui lòng nhập Tên Nhóm hàng!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtName.Focus();

                return;
            }

            category.Name =
                txtName.Text.Trim();

            category.Description =
                txtDescription.Text.Trim();

            DisplayCategories(categories);

            SelectCategoryByID(id);

            lblStatus.Text =
                "Đã cập nhật nhóm hàng";

            MessageBox.Show(
                "Cập nhật thành công!",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }


        // =====================================================
        // XÓA
        // =====================================================

        private void btnDelete_Click(
            object sender,
            EventArgs e)
        {
            int id;

            if (!int.TryParse(
                txtID.Text,
                out id))
            {
                MessageBox.Show(
                    "Vui lòng chọn nhóm hàng cần xóa!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            Category category =
                categories.FirstOrDefault(
                    x => x.ID == id);

            if (category == null)
            {
                return;
            }

            DialogResult result =
                MessageBox.Show(
                    "Bạn có chắc muốn xóa nhóm hàng \""
                    + category.Name
                    + "\" không?",

                    "Xác nhận xóa",

                    MessageBoxButtons.YesNo,

                    MessageBoxIcon.Question
                );

            if (result ==
                DialogResult.Yes)
            {
                categories.Remove(category);

                DisplayCategories(categories);

                SelectFirstRow();

                lblStatus.Text =
                    "Đã xóa nhóm hàng";

                MessageBox.Show(
                    "Xóa thành công!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }


        // =====================================================
        // CHỌN NHÓM HÀNG THEO ID
        // =====================================================

        private void SelectCategoryByID(
            int id)
        {
            for (int i = 0;
                 i < dataGridView1.Rows.Count;
                 i++)
            {
                string currentID =
                    Convert.ToString(
                        dataGridView1.Rows[i]
                            .Cells[0].Value);

                if (currentID ==
                    id.ToString())
                {
                    dataGridView1.ClearSelection();

                    dataGridView1.Rows[i].Selected =
                        true;

                    dataGridView1.CurrentCell =
                        dataGridView1.Rows[i].Cells[0];

                    ShowCategory(i);

                    return;
                }
            }
        }


        // =====================================================
        // PLACEHOLDER - ENTER
        // =====================================================

        private void txtSearch_Enter(
            object sender,
            EventArgs e)
        {
            if (txtSearch.Text ==
                "Nhập từ khóa...")
            {
                txtSearch.Clear();

                txtSearch.ForeColor =
                    Color.Black;
            }
        }


        // =====================================================
        // PLACEHOLDER - LEAVE
        // =====================================================

        private void txtSearch_Leave(
            object sender,
            EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                txtSearch.Text))
            {
                txtSearch.Text =
                    "Nhập từ khóa...";

                txtSearch.ForeColor =
                    Color.Gray;
            }
        }


        // =====================================================
        // MODEL CATEGORY
        // =====================================================

        private class Category
        {
            public int ID { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }
        }
    }
}
