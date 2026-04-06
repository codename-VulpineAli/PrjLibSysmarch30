<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="prjLibrarySystem.Settings" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Settings - Library Management System</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet" />
    <style>
        .main-content { padding: 20px; }
        .nav-tabs .nav-link { cursor: pointer; }
    </style>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js"></script>
    <script>
        function sendDueDateReminders() { __doPostBack('btnSendReminders', ''); }
    </script>
</head>
<body>
<form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <div class="container-fluid">
        <div class="row">
            <!-- ================= SIDEBAR ================= -->
            <asp:Literal ID="litSidebar" runat="server"></asp:Literal>

            <!-- ================= MAIN CONTENT ================= -->
            <main class="col-12 col-md-9 col-lg-10 px-md-4 main-content">
                <div class="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
                    <h1 class="h2"><i class="fas fa-cogs me-2"></i> Settings</h1>
                    <asp:Label ID="lblMsg" runat="server"></asp:Label>
                    <div class="btn-toolbar mb-2 mb-md-0 align-items-center gap-3">
                        <div class="btn-group me-2">
                            <div class="dropdown position-relative">
                                <button class="btn btn-sm btn-outline-secondary dropdown-toggle" type="button"
                                        id="adminNotificationDropdown" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="fas fa-bell"></i>
                                    <asp:Label ID="adminNotificationBadge" runat="server"
                                               CssClass="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-warning">0</asp:Label>
                                </button>
                                <ul class="dropdown-menu dropdown-menu-end">
                                    <li><h6 class="dropdown-header">System Notifications</h6></li>
                                    <li><hr class="dropdown-divider"></li>
                                    <li><a class="dropdown-item" href="#" onclick="sendDueDateReminders()"><i class="fas fa-envelope me-2"></i>Send Due Date Reminders</a></li>
                                    <li><a class="dropdown-item" href="#"><i class="fas fa-book me-2"></i>Low Stock Alerts</a></li>
                                    <li><a class="dropdown-item" href="#"><i class="fas fa-chart-line me-2"></i>Usage Report</a></li>
                                    <li><hr class="dropdown-divider"></li>
                                    <li><a class="dropdown-item" href="#" data-bs-toggle="modal" data-bs-target="#notificationsModal">View All Notifications</a></li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- ================= SETTINGS TABS ================= -->
                <ul class="nav nav-tabs">
                    <li class="nav-item">
                        <button type="button" class="nav-link active" data-bs-toggle="tab" data-bs-target="#categories">Book Categories</button>
                    </li>
                    <li class="nav-item">
                        <button type="button" class="nav-link" data-bs-toggle="tab" data-bs-target="#yearlevel">Year Level</button>
                    </li>
                    <li class="nav-item">
                        <button type="button" class="nav-link" data-bs-toggle="tab" data-bs-target="#course">Course</button>
                    </li>
                    <li class="nav-item">
                        <button type="button" class="nav-link" data-bs-toggle="tab" data-bs-target="#policy">Borrow Policy</button>
                    </li>
                </ul>

                <div class="tab-content mt-3">
                        </div>
                    <!-- ================= CATEGORY ================= -->
                    <div class="tab-pane fade show active" id="categories">
                        <div class="mb-3">
                            <asp:TextBox ID="txtCategory" runat="server" CssClass="form-control mb-2" placeholder="Add Category"></asp:TextBox>
                            <asp:Button ID="btnAddCategory" runat="server" Text="Add Category" CssClass="btn btn-primary" OnClick="btnAddCategory_Click" />
                        </div>
                        <asp:GridView ID="gvCategories" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered table-striped">
                            <Columns>
                                <asp:BoundField DataField="CategoryName" HeaderText="Category" />
                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate>
                                        <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:Button runat="server" Text="Edit" CssClass="btn btn-info btn-sm me-1" CommandArgument='<%# Eval("CategoryID") %>' OnClick="btnEditCategory_Click" />
                                        <asp:Button runat="server" Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "Deactivate" : "Activate" %>' CssClass="btn btn-warning btn-sm" CommandArgument='<%# Eval("CategoryID") %>' OnClick="btnToggleCategory_Click" />
                                        <asp:Button runat="server" Text="Delete" CssClass="btn btn-danger btn-sm" CommandArgument='<%# Eval("CategoryID") %>' OnClick="btnDeleteCategory_Click" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

                    <!-- ================= YEAR LEVEL ================= -->
                    <div class="tab-pane fade" id="yearlevel">
                        <div class="mb-3">
                            <asp:TextBox ID="txtYearLevel" runat="server" CssClass="form-control mb-2" placeholder="Add Year Level"></asp:TextBox>
                            <asp:Button ID="btnAddYear" runat="server" Text="Add Year Level" CssClass="btn btn-primary" OnClick="btnAddYear_Click" />
                        </div>
                        <asp:GridView ID="gvYearLevel" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered table-striped">
                            <Columns>
                                <asp:BoundField DataField="YearLevelName" HeaderText="Year Level" />
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:Button runat="server" Text="Edit" CssClass="btn btn-info btn-sm me-1" CommandArgument='<%# Eval("YearLevelID") %>' OnClick="btnEditYear_Click" />
                                        <asp:Button runat="server" Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "Deactivate" : "Activate" %>' CssClass="btn btn-warning btn-sm" CommandArgument='<%# Eval("YearLevelID") %>' OnClick="btnToggleYear_Click" />
                                        <asp:Button runat="server" Text="Delete" CssClass="btn btn-danger btn-sm" CommandArgument='<%# Eval("YearLevelID") %>' OnClick="btnDeleteYear_Click" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

                    <!-- ================= COURSE ================= -->
                    <div class="tab-pane fade" id="course">
                        <div class="mb-3">
                            <asp:TextBox ID="txtCourse" runat="server" CssClass="form-control mb-2" placeholder="Add Course"></asp:TextBox>
                            <asp:Button ID="btnAddCourse" runat="server" Text="Add Course" CssClass="btn btn-primary" OnClick="btnAddCourse_Click" />
                        </div>
                        <asp:GridView ID="gvCourse" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered table-striped">
                            <Columns>
                                <asp:BoundField DataField="CourseName" HeaderText="Course" />
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:Button runat="server" Text="Edit" CssClass="btn btn-info btn-sm me-1" CommandArgument='<%# Eval("CourseID") %>' OnClick="btnEditCourse_Click" />
                                        <asp:Button runat="server" Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "Deactivate" : "Activate" %>' CssClass="btn btn-warning btn-sm" CommandArgument='<%# Eval("CourseID") %>' OnClick="btnToggleCourse_Click" />
                                        <asp:Button runat="server" Text="Delete" CssClass="btn btn-danger btn-sm" CommandArgument='<%# Eval("CourseID") %>' OnClick="btnDeleteCourse_Click" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

<!-- ================= POLICY ================= -->
<div class="tab-pane fade" id="policy">
    
    <asp:GridView ID="gvPolicy" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered table-striped">
        <Columns>
            <asp:BoundField DataField="MemberType" HeaderText="Member Type" />
            <asp:BoundField DataField="SettingKey" HeaderText="Setting" />
            <asp:BoundField DataField="SettingValue" HeaderText="Value" />
            <asp:TemplateField HeaderText="Action">
                <ItemTemplate>
                    <asp:Button runat="server" Text="Edit" CssClass="btn btn-primary btn-sm"
                        CommandArgument='<%# Eval("SettingID") %>'
                        OnClick="btnEditPolicy_Click" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</div>
    <!-- ================= EDIT MODAL ================= -->
    <div class="modal fade" id="editModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5>Edit</h5>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hfID" runat="server" />
                    <asp:TextBox ID="txtEditValue" runat="server" CssClass="form-control" />
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnUpdate" runat="server" Text="Update" CssClass="btn btn-primary" OnClick="btnUpdate_Click" />
                </div>
            </div>
        </div>
    </div>
</form>
</body>
</html>
