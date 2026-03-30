using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using prjLibrarySystem.Models;

namespace prjLibrarySystem
{
    public partial class AdminMembers : System.Web.UI.Page
    {
        private string SearchTerm
        {
            get { return ViewState["SearchTerm"] as string ?? ""; }
            set { ViewState["SearchTerm"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) { Response.Redirect("Login.aspx"); return; }
            string role = Session["Role"]?.ToString();
            if (role != "Admin" && role != "Super Admin") { Response.Redirect("MemberDashboard.aspx"); return; }

            litSidebar.Text = SidebarHelper.GetSidebar(role, "members");

            lblStatusAlert.Visible = false;
            lblStatusAlert.Text = "";
            lblStatusAlert.CssClass = "alert d-none";

            if (!IsPostBack) LoadMembers();
        }

        private void LoadMembers()
        {

            try
            {
                var parameters = new List<SqlParameter>();

                // Fixed: Course -> CourseID/CourseName join, YearLevel -> YearLevelID/YearLevelName join
                string query = @"
                    SELECT
                        m.MemberID,
                        m.FullName,
                        u.UserID        AS Username,
                        u.Email,
                        c.CourseName    AS Course,
                        yl.YearLevelName AS YearLevel,
                        m.MemberType    AS Role,
                        u.CreatedAt     AS RegistrationDate,
                        u.IsActive,
                        CASE WHEN u.IsActive = 1 THEN 'Active' ELSE 'Inactive' END AS Status
                    FROM tblMembers m
                    INNER JOIN tblUsers u ON m.UserID = u.UserID
                    LEFT  JOIN tblCourses    c  ON c.CourseID    = m.CourseID
                    LEFT  JOIN tblYearLevels yl ON yl.YearLevelID = m.YearLevelID
                    WHERE u.Role = 'Member'";

                if (!string.IsNullOrEmpty(ddlMembershipType.SelectedValue))
                {
                    query += " AND m.MemberType = @MemberType";
                    parameters.Add(new SqlParameter("@MemberType", ddlMembershipType.SelectedValue));
                }
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    query += " AND (m.FullName LIKE @Search OR u.Email LIKE @Search OR u.UserID LIKE @Search)";
                    parameters.Add(new SqlParameter("@Search", "%" + SearchTerm + "%"));
                }
                if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                {
                    query += " AND (CASE WHEN u.IsActive = 1 THEN 'Active' ELSE 'Inactive' END) = @Status";
                    parameters.Add(new SqlParameter("@Status", ddlStatus.SelectedValue));
                }

                query += " ORDER BY m.MemberType ASC, u.UserID ASC";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters.ToArray());
                gvMembers.DataSource = dt;
                gvMembers.DataBind();
                txtSearchMember.Text = SearchTerm;
            }
            catch (Exception ex)
            {
                gvMembers.DataSource = null;
                gvMembers.DataBind();
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    $"alert('Error loading members: {ex.Message}');", true);
            }
        }

        private void LoadCourses()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery(
                "SELECT CourseID, CourseName FROM tblCourses WHERE IsActive = 1",
                null);

            ddlCourse.DataSource = dt;
            ddlCourse.DataTextField = "CourseName";
            ddlCourse.DataValueField = "CourseID";
            ddlCourse.DataBind();

            ddlCourse.Items.Insert(0, new ListItem("-- Select Course --", ""));
        }

        private void LoadYearLevels()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery(
                "SELECT YearLevelID, YearLevelName FROM tblYearLevels WHERE IsActive = 1",
                null);

            ddlYearLevel.DataSource = dt;
            ddlYearLevel.DataTextField = "YearLevelName";
            ddlYearLevel.DataValueField = "YearLevelID";
            ddlYearLevel.DataBind();

            ddlYearLevel.Items.Insert(0, new ListItem("-- Select Year Level --", ""));
        }

        protected void btnAddNewMember_Click(object sender, EventArgs e)
        {
            LoadCourses();
            LoadYearLevels();

            hfEditingMemberId.Value = string.Empty;
            txtUserId.Text = string.Empty;
            txtFullName.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtPassword.Text = string.Empty;
            ddlCourse.SelectedIndex = 0;
            ddlYearLevel.SelectedIndex = 0;
            txtUserId.ReadOnly = false;
            hfSelectedRole.Value = "Student";

            btnStudentRole.Style["display"] = "block";
            btnTeacherRole.Style["display"] = "block";

            ScriptManager.RegisterStartupScript(this, GetType(), "resetRole",
                "selectMemberType('Student');", true);
            ScriptManager.RegisterStartupScript(this, GetType(), "showModal",
                "new bootstrap.Modal(document.getElementById('memberModal')).show();", true);
        }

        protected void btnSaveMember_Click(object sender, EventArgs e)
        {
            try
            {
                string memberType = hfSelectedRole.Value;
                if (memberType != "Student" && memberType != "Teacher") memberType = "Student";

                string adminId = Session["UserID"]?.ToString() ?? "";
                string adminName = Session["FullName"]?.ToString() ?? "";

                if (!string.IsNullOrEmpty(hfEditingMemberId.Value))
                {
                    // ── Edit ──────────────────────────────────────────────────
                    int memberId = int.Parse(hfEditingMemberId.Value);

                    DataTable userDt = DatabaseHelper.ExecuteQuery(
                        "SELECT UserID FROM tblMembers WHERE MemberID = @MemberID",
                        new SqlParameter[] { new SqlParameter("@MemberID", memberId) });

                    if (userDt.Rows.Count == 0) throw new Exception("Member not found.");
                    string userId = userDt.Rows[0]["UserID"].ToString();

                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE tblUsers SET Email=@Email WHERE UserID=@UserID",
                        new SqlParameter[]
                        {
                            new SqlParameter("@Email",  txtEmail.Text),
                            new SqlParameter("@UserID", userId)
                        });

                    if (!string.IsNullOrEmpty(txtPassword.Text))
                        DatabaseHelper.ExecuteNonQuery(
                            "UPDATE tblUsers SET PasswordHash=@PasswordHash WHERE UserID=@UserID",
                            new SqlParameter[]
                            {
                                new SqlParameter("@PasswordHash", DatabaseHelper.HashPassword(txtPassword.Text)),
                                new SqlParameter("@UserID",       userId)
                            });

                    if (memberType == "Teacher")
                        // Fixed: NULL out CourseID and YearLevelID (FK ints) for teachers; stamp UpdatedAt
                        DatabaseHelper.ExecuteNonQuery(
                            "UPDATE tblMembers SET FullName=@FullName, MemberType='Teacher', CourseID=NULL, YearLevelID=NULL, UpdatedAt=GETDATE() WHERE MemberID=@MemberID",
                            new SqlParameter[]
                            {
                                new SqlParameter("@FullName", txtFullName.Text),
                                new SqlParameter("@MemberID", memberId)
                            });
                    else
                        // Fixed: CourseID and YearLevelID are FK ints, not strings
                        DatabaseHelper.ExecuteNonQuery(
                            "UPDATE tblMembers SET FullName=@FullName, MemberType='Student', CourseID=@CourseID, YearLevelID=@YearLevelID, UpdatedAt=GETDATE() WHERE MemberID=@MemberID",
                            new SqlParameter[]
                            {
                                new SqlParameter("@FullName",    txtFullName.Text),
                                new SqlParameter("@CourseID",    ParseCourseId(ddlCourse.SelectedValue)),
                                new SqlParameter("@YearLevelID", ParseYearLevelId(ddlYearLevel.SelectedValue)),
                                new SqlParameter("@MemberID",    memberId)
                            });

                    DatabaseHelper.WriteAuditLog(adminId, adminName, "EDIT_MEMBER", "tblMembers", memberId.ToString());
                }
                else
                {
                    // ── Add ───────────────────────────────────────────────────
                    string newUserId = txtUserId.Text.Trim();

                    if (string.IsNullOrEmpty(newUserId) || string.IsNullOrEmpty(txtPassword.Text))
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "validation",
                            "alert('User ID and Password are required.');", true);
                        return;
                    }

                    int exists = Convert.ToInt32(DatabaseHelper.ExecuteScalar(
                        "SELECT COUNT(*) FROM tblUsers WHERE UserID=@UserID",
                        new SqlParameter[] { new SqlParameter("@UserID", newUserId) }));

                    if (exists > 0)
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "dupError",
                            "alert('User ID already exists. Please choose a different one.');", true);
                        return;
                    }

                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO tblUsers (UserID, PasswordHash, Role, Email, IsActive) VALUES (@UserID, @PasswordHash, 'Member', @Email, 1)",
                        new SqlParameter[]
                        {
                            new SqlParameter("@UserID",       newUserId),
                            new SqlParameter("@PasswordHash", DatabaseHelper.HashPassword(txtPassword.Text)),
                            new SqlParameter("@Email",        txtEmail.Text)
                        });

                    if (memberType == "Teacher")
                        // Fixed: INSERT without CourseID/YearLevelID (they default to NULL for teachers)
                        DatabaseHelper.ExecuteNonQuery(
                            "INSERT INTO tblMembers (UserID, FullName, MemberType) VALUES (@UserID, @FullName, 'Teacher')",
                            new SqlParameter[]
                            {
                                new SqlParameter("@UserID",   newUserId),
                                new SqlParameter("@FullName", txtFullName.Text)
                            });
                    else
                        // Fixed: use CourseID (int FK) and YearLevelID (int FK)
                        DatabaseHelper.ExecuteNonQuery(
                            "INSERT INTO tblMembers (UserID, FullName, MemberType, CourseID, YearLevelID) VALUES (@UserID, @FullName, 'Student', @CourseID, @YearLevelID)",
                            new SqlParameter[]
                            {
                                new SqlParameter("@UserID",      newUserId),
                                new SqlParameter("@FullName",    txtFullName.Text),
                                new SqlParameter("@CourseID",    ParseCourseId(ddlCourse.SelectedValue)),
                                new SqlParameter("@YearLevelID", ParseYearLevelId(ddlYearLevel.SelectedValue))
                            });

                    DatabaseHelper.WriteAuditLog(adminId, adminName, "ADD_MEMBER", "tblMembers", newUserId);
                }

                hfEditingMemberId.Value = "";
                LoadMembers();

                ScriptManager.RegisterStartupScript(this, GetType(), "closeModal",
                    "var m=bootstrap.Modal.getInstance(document.getElementById('memberModal'));if(m)m.hide();", true);
                ScriptManager.RegisterStartupScript(this, GetType(), "success",
                    "alert('Member saved successfully.');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    $"alert('Error saving member: {ex.Message}');", true);
            }
        }

        protected void btnSearchMember_Click(object sender, EventArgs e)
        {
            SearchTerm = txtSearchMember.Text.Trim();
            gvMembers.PageIndex = 0;
            LoadMembers();
        }

        protected void ddlMembershipType_SelectedIndexChanged(object sender, EventArgs e) { gvMembers.PageIndex = 0; LoadMembers(); }
        protected void ddlStatus_SelectedIndexChanged(object sender, EventArgs e) { gvMembers.PageIndex = 0; LoadMembers(); }

        protected void gvMembers_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvMembers.PageIndex = e.NewPageIndex;
            LoadMembers();
        }

        protected void gvMembers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Page" || e.CommandName == "Sort") return;

            try
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument.ToString());
                string userId = gvMembers.DataKeys[rowIndex]["Username"].ToString();
                string memberId = gvMembers.DataKeys[rowIndex]["MemberID"].ToString();
                string adminId = Session["UserID"]?.ToString() ?? "";
                string adminName = Session["FullName"]?.ToString() ?? "";

                if (e.CommandName == "ToggleStatus")
                {
                    DataTable userDt = DatabaseHelper.ExecuteQuery(
                        "SELECT IsActive FROM tblUsers WHERE UserID=@UserID",
                        new SqlParameter[] { new SqlParameter("@UserID", userId) });

                    if (userDt.Rows.Count > 0)
                    {
                        int currentActive = Convert.ToInt32(userDt.Rows[0]["IsActive"]);
                        int newActive = currentActive == 1 ? 0 : 1;

                        if (currentActive == 1)
                        {
                            int activeCount = Convert.ToInt32(DatabaseHelper.ExecuteScalar(
                                "SELECT COUNT(*) FROM tblTransactions WHERE MemberID=@MemberID AND Status='Active'",
                                new SqlParameter[] { new SqlParameter("@MemberID", memberId) }));

                            if (activeCount > 0)
                            {
                                ShowAlert("Cannot deactivate this member. They still have active borrow transactions.", "danger");
                                return;
                            }
                        }

                        DatabaseHelper.ExecuteNonQuery(
                            "UPDATE tblUsers SET IsActive=@IsActive WHERE UserID=@UserID",
                            new SqlParameter[]
                            {
                                new SqlParameter("@IsActive", newActive),
                                new SqlParameter("@UserID",   userId)
                            });
                        DatabaseHelper.WriteAuditLog(adminId, adminName,
                            newActive == 1 ? "ACTIVATE_MEMBER" : "DEACTIVATE_MEMBER", "tblUsers", userId);
                    }
                    LoadMembers();
                    return;
                }

                if (e.CommandName == "DeleteMember")
                {
                    int activeCount = Convert.ToInt32(DatabaseHelper.ExecuteScalar(
                        "SELECT COUNT(*) FROM tblTransactions WHERE MemberID=@MemberID AND Status='Active'",
                        new SqlParameter[] { new SqlParameter("@MemberID", memberId) }));

                    if (activeCount > 0)
                    {
                        ShowAlert("Cannot delete this member. They still have active borrow transactions.", "danger");
                        return;
                    }

                    DatabaseHelper.WriteAuditLog(adminId, adminName, "DELETE_MEMBER", "tblUsers", userId);
                    // ON DELETE CASCADE in tblMembers handles the member row automatically
                    DatabaseHelper.ExecuteNonQuery("DELETE FROM tblUsers WHERE UserID=@UserID",
                        new SqlParameter[] { new SqlParameter("@UserID", userId) });

                    LoadMembers();
                    ScriptManager.RegisterStartupScript(this, GetType(), "success",
                        "alert('Member deleted successfully.');", true);
                    return;
                }

                if (e.CommandName == "EditMember")
                {
                    // Fixed: select CourseID and YearLevelID (int FKs) for editing
                    DataTable dt = DatabaseHelper.ExecuteQuery(@"
                        SELECT m.MemberID, m.FullName, m.MemberType, m.CourseID, m.YearLevelID,
                               u.UserID, u.Email
                        FROM tblMembers m
                        INNER JOIN tblUsers u ON m.UserID = u.UserID
                        WHERE u.UserID = @UserID",
                        new SqlParameter[] { new SqlParameter("@UserID", userId) });

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        string mtype = row["MemberType"].ToString();

                        txtUserId.Text = row["UserID"].ToString();
                        txtFullName.Text = row["FullName"].ToString();
                        txtEmail.Text = row["Email"].ToString();
                        txtPassword.Text = "";
                        hfEditingMemberId.Value = row["MemberID"].ToString();
                        lblRegisterTitle.Text = "Edit Member";
                        txtUserId.ReadOnly = true;
                        hfSelectedRole.Value = mtype;

                        if (mtype == "Teacher")
                        {
                            ddlCourse.SelectedIndex = 0;
                            ddlYearLevel.SelectedIndex = 0;
                            btnStudentRole.Style["display"] = "none";
                            btnTeacherRole.Style["display"] = "block";
                            ScriptManager.RegisterStartupScript(this, GetType(), "setType",
                                "selectMemberType('Teacher');", true);
                        }
                        else
                        {
                            // Fixed: set CourseID and YearLevelID int values on dropdowns
                            if (row["CourseID"] != DBNull.Value)
                                ddlCourse.SelectedValue = row["CourseID"].ToString();

                            if (row["YearLevelID"] != DBNull.Value)
                                ddlYearLevel.SelectedValue = row["YearLevelID"].ToString();

                            btnStudentRole.Style["display"] = "block";
                            btnTeacherRole.Style["display"] = "none";
                            ScriptManager.RegisterStartupScript(this, GetType(), "setType",
                                "selectMemberType('Student');", true);
                        }

                        ScriptManager.RegisterStartupScript(this, GetType(), "openModal",
                            "new bootstrap.Modal(document.getElementById('memberModal')).show();", true);
                    }
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    $"alert('Error: {ex.Message}');", true);
            }
        }

        protected string GetStatusBadgeClass(object statusObj) =>
            statusObj?.ToString() == "Active" ? "status-active" : "status-inactive";

        private void ShowAlert(string message, string type)
        {
            lblStatusAlert.Text = message;
            lblStatusAlert.CssClass = $"alert alert-{type}";
            lblStatusAlert.Visible = true;
            ScriptManager.RegisterStartupScript(this, GetType(), "hideAlert",
                $"setTimeout(function(){{ document.getElementById('{lblStatusAlert.ClientID}').style.display='none'; }}, 5000);", true);
        }

        // Fixed: ddlCourse values should be CourseIDs (ints from tblCourses)
        private object ParseCourseId(string value)
        {
            if (int.TryParse(value, out int id) && id > 0) return id;
            return DBNull.Value;
        }

        // Fixed: ddlYearLevel values should be YearLevelIDs (ints from tblYearLevels)
        private object ParseYearLevelId(string value)
        {
            if (int.TryParse(value, out int id) && id > 0) return id;
            return DBNull.Value;
        }
    }
}