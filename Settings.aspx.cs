using prjLibrarySystem.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace prjLibrarySystem
{
    public partial class Settings : System.Web.UI.Page
    {
        // ================== ViewState Properties ==================
        string CurrentTable
        {
            get => ViewState["CurrentTable"]?.ToString();
            set => ViewState["CurrentTable"] = value;
        }

        string CurrentField
        {
            get => ViewState["CurrentField"]?.ToString();
            set => ViewState["CurrentField"] = value;
        }

        string CurrentID
        {
            get => ViewState["CurrentID"]?.ToString();
            set => ViewState["CurrentID"] = value;
        }

        // ================== Page Load ==================
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) { Response.Redirect("Login.aspx"); return; }
            string role = Session["Role"]?.ToString();
            if (role != "Admin") { Response.Redirect("MemberDashboard.aspx"); return; }

            litSidebar.Text = SidebarHelper.GetSidebar(role, "settings");

            if (!IsPostBack)
            {
                LoadPolicy();
                LoadCategories();
                LoadYearLevel();
                LoadCourses();
            }
        }

        // ================== LOAD DATA ==================
        void LoadPolicy()
        {
            gvPolicy.DataSource = DatabaseHelper.ExecuteQuery(
                "SELECT SettingID, MemberType, SettingKey, SettingValue FROM tblBorrowPolicies",
                null);
            gvPolicy.DataBind();
        }

        void LoadCategories()
        {
            gvCategories.DataSource = DatabaseHelper.ExecuteQuery("SELECT * FROM tblCategories", null);
            gvCategories.DataBind();
        }

        void LoadYearLevel()
        {
            gvYearLevel.DataSource = DatabaseHelper.ExecuteQuery("SELECT * FROM tblYearLevels", null);
            gvYearLevel.DataBind();
        }

        void LoadCourses()
        {
            gvCourse.DataSource = DatabaseHelper.ExecuteQuery("SELECT * FROM tblCourses", null);
            gvCourse.DataBind();
        }

        // ================== CATEGORY ==================
        protected void btnAddCategory_Click(object sender, EventArgs e)
        {
            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO tblCategories(CategoryName, IsActive) VALUES(@name,1)",
                new SqlParameter[] { new SqlParameter("@name", txtCategory.Text) });

            txtCategory.Text = "";
            LoadCategories();
        }

        protected void btnToggleCategory_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).CommandArgument);
            DatabaseHelper.ExecuteNonQuery(@"
                UPDATE tblCategories
                SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
                WHERE CategoryID=@id",
                new SqlParameter[] { new SqlParameter("@id", id) });

            LoadCategories();
        }

        protected void btnDeleteCategory_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).CommandArgument);

            DataTable dt = DatabaseHelper.ExecuteQuery(
                "SELECT COUNT(*) FROM tblBooks WHERE CategoryID=@id",
                new SqlParameter[] { new SqlParameter("@id", id) });

            if (Convert.ToInt32(dt.Rows[0][0]) > 0)
            {
                ScriptManager.RegisterStartupScript(this, GetType(),
                    "alert", "alert('Category in use!');", true);
                return;
            }

            DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM tblCategories WHERE CategoryID=@id",
                new SqlParameter[] { new SqlParameter("@id", id) });

            LoadCategories();
        }

        // ================== YEAR LEVEL ==================
        protected void btnAddYear_Click(object sender, EventArgs e)
        {
            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO tblYearLevels(YearLevelName, IsActive) VALUES(@name,1)",
                new SqlParameter[] { new SqlParameter("@name", txtYearLevel.Text) });

            txtYearLevel.Text = "";
            LoadYearLevel();
        }

        protected void btnToggleYear_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).CommandArgument);
            DatabaseHelper.ExecuteNonQuery(@"
                UPDATE tblYearLevels
                SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
                WHERE YearLevelID=@id",
                new SqlParameter[] { new SqlParameter("@id", id) });

            LoadYearLevel();
        }

        protected void btnDeleteYear_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).CommandArgument);

            DataTable dt = DatabaseHelper.ExecuteQuery(
                "SELECT COUNT(*) FROM tblMembers WHERE YearLevelID=@id",
                new SqlParameter[] { new SqlParameter("@id", id) });

            if (Convert.ToInt32(dt.Rows[0][0]) > 0)
            {
                ScriptManager.RegisterStartupScript(this, GetType(),
                    "alert", "alert('Year Level in use!');", true);
                return;
            }

            DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM tblYearLevels WHERE YearLevelID=@id",
                new SqlParameter[] { new SqlParameter("@id", id) });

            LoadYearLevel();
        }

        // ================== COURSE ==================
        protected void btnAddCourse_Click(object sender, EventArgs e)
        {
            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO tblCourses(CourseName, IsActive) VALUES(@name,1)",
                new SqlParameter[] { new SqlParameter("@name", txtCourse.Text) });

            txtCourse.Text = "";
            LoadCourses();
        }

        protected void btnToggleCourse_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).CommandArgument);
            DatabaseHelper.ExecuteNonQuery(@"
                UPDATE tblCourses
                SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
                WHERE CourseID=@id",
                new SqlParameter[] { new SqlParameter("@id", id) });

            LoadCourses();
        }

        protected void btnDeleteCourse_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).CommandArgument);

            DataTable dt = DatabaseHelper.ExecuteQuery(
                "SELECT COUNT(*) FROM tblMembers WHERE CourseID=@id",
                new SqlParameter[] { new SqlParameter("@id", id) });

            if (Convert.ToInt32(dt.Rows[0][0]) > 0)
            {
                ScriptManager.RegisterStartupScript(this, GetType(),
                    "alert", "alert('Course in use!');", true);
                return;
            }

            DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM tblCourses WHERE CourseID=@id",
                new SqlParameter[] { new SqlParameter("@id", id) });

            LoadCourses();
        }

        // ================== EDIT BUTTONS ==================
        protected void btnEditCategory_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).CommandArgument);
            CurrentTable = "tblCategories";
            CurrentField = "CategoryName";
            CurrentID = "CategoryID";
            LoadEditData(id);
        }

        protected void btnEditYear_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).CommandArgument);
            CurrentTable = "tblYearLevels";
            CurrentField = "YearLevelName";
            CurrentID = "YearLevelID";
            LoadEditData(id);
        }

        protected void btnEditCourse_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).CommandArgument);
            CurrentTable = "tblCourses";
            CurrentField = "CourseName";
            CurrentID = "CourseID";
            LoadEditData(id);
        }

        protected void btnEditPolicy_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).CommandArgument);
            CurrentTable = "tblBorrowPolicies";
            CurrentField = "SettingValue";
            CurrentID = "SettingID";
            LoadEditData(id);
        }

        // ================== LOAD EDIT DATA ==================
        void LoadEditData(int id)
        {
            DataTable dt = DatabaseHelper.ExecuteQuery(
                $"SELECT {CurrentField} FROM {CurrentTable} WHERE {CurrentID}=@id",
                new SqlParameter[] { new SqlParameter("@id", id) });

            hfID.Value = id.ToString();
            txtEditValue.Text = dt.Rows[0][0].ToString();

            ScriptManager.RegisterStartupScript(this, GetType(),
                "Popup", "$('#editModal').modal('show');", true);
        }

        // ================== UPDATE BUTTON ==================
        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(hfID.Value) || string.IsNullOrEmpty(CurrentTable)
                || string.IsNullOrEmpty(CurrentField) || string.IsNullOrEmpty(CurrentID))
            {
                ShowMessage("Invalid operation!", "danger");
                return;
            }

            int id = Convert.ToInt32(hfID.Value);

            DatabaseHelper.ExecuteNonQuery(
                $"UPDATE {CurrentTable} SET {CurrentField}=@value WHERE {CurrentID}=@id",
                new SqlParameter[] {
                    new SqlParameter("@value", txtEditValue.Text),
                    new SqlParameter("@id", id)
                }
            );

            ShowMessage("Updated successfully!");

            switch (CurrentTable)
            {
                case "tblCategories": LoadCategories(); break;
                case "tblYearLevels": LoadYearLevel(); break;
                case "tblCourses": LoadCourses(); break;
                case "tblBorrowPolicies": LoadPolicy(); break;
            }

            ScriptManager.RegisterStartupScript(this, GetType(), "CloseModal", "$('#editModal').modal('hide');", true);
        }

        // ================== SHOW MESSAGE ==================
        void ShowMessage(string msg, string type = "success")
        {
            lblMsg.Text = msg;
            lblMsg.CssClass = "alert alert-" + type;
        }
    }
}
