<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="UsersList.aspx.cs"
    Inherits="UserWebApp.Users.UsersList"
    MasterPageFile="~/Site.Master" 
    Async="true" %>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">

    <!-- SweetAlert2 CDN -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        .page-wrap {
            max-width:1250px;
            margin:24px auto;
            padding:24px;
            background:#ffffff;
            border-radius:14px;
            box-shadow:0 6px 20px rgba(0,0,0,0.07);
        }
        .page-wrap h3 { font-size:28px; margin-bottom:18px; color:#333; font-weight:700; }

        .upload-area { display:flex; align-items:center; gap:12px; margin-bottom:20px; flex-wrap:wrap; }
        .upload-area input[type=file] { padding:8px; border:1px solid #d1d5db; border-radius:8px; min-width:320px; }
        .btn { border-radius:10px; padding:8px 18px; font-weight:600; cursor:pointer; border:none; }
        .btn-primary { background:#4ea7ff; color:#fff; }
        .btn-secondary { background:#6c757d; color:#fff; }
        .btn-danger { background:#ff6b6b; color:#fff; }

        .table-responsive { width:100%; overflow-x:auto; border-radius:10px; }
        .table { width:100%; border-collapse:collapse; background:#fff; min-width:900px; }
        .table thead th { background:#222831; color:white; padding:12px; font-weight:700; border-bottom:3px solid #1b1f23; }
        .table tbody td { padding:12px; border-bottom:1px solid #ececec; color:#222; }

        .action-buttons { display:flex; flex-wrap:nowrap !important; gap:10px; align-items:center; justify-content:flex-end; white-space:nowrap; }
        .action-buttons .btn { padding:7px 14px; font-size:13px; border-radius:12px; margin:0; line-height:1; min-width:60px; white-space:nowrap; }
        .btn-info { background:#17c0eb; color:#fff; }
        .btn-warning { background:#ffca28; color:#000; }
        .text-danger { color:#d9534f; font-weight:600; }
        .text-success { color:green; font-weight:600; }
        .upload-summary { margin-left:12px; font-weight:600; }
        .template-box { margin-top:18px; padding:18px; border-radius:8px; background:#fafafa; border:1px solid #eee; }

        /* ------------------------------
           SUMMARY BOX (new responsive)
           ------------------------------ */
        .upload-summary-box {
            display: flex;
            gap: 18px;
            align-items: flex-start;
            flex-wrap: wrap;
            margin: 12px 0 18px 0;
        }
        .upload-summary-box .summary-left {
            min-width: 140px;
            max-width: 180px;
            flex: 0 0 auto;
            display: flex;
            flex-direction: column;
            gap: 6px;
        }
        .upload-summary-box .summary-right {
            flex: 1 1 600px;
            min-width: 200px;
        }
        .upload-summary-box .summary-right .text-success,
        .upload-summary-box .summary-right .text-danger { font-weight:700; }
        .upload-summary-box .summary-right ul { margin: 8px 0; padding-left: 20px; max-height: 160px; overflow: auto; }
        .upload-summary-box .summary-right,
        .upload-summary-box .summary-right li { word-break: break-word; overflow-wrap: anywhere; }
        .upload-summary-box .summary-right a { display:inline-block; margin-top:6px; }

        @media (max-width: 720px) {
            .upload-summary-box { gap: 10px; }
            .upload-summary-box .summary-left { min-width: auto; width: 100%; order: 2; }
            .upload-summary-box .summary-right { order: 1; width: 100%; }
        }
    </style>

    <div class="page-wrap">

        <!-- Upload & Sample -->
        <div class="upload-area">
            <asp:FileUpload ID="fuExcel" runat="server" />
            <asp:Button ID="btnUploadExcel" runat="server" Text="Upload Excel" CssClass="btn btn-primary" OnClick="btnUploadExcel_Click" />
            <asp:Button ID="btnDownloadSample" runat="server" Text="Download Sample (.xlsx)" CssClass="btn btn-secondary" OnClick="btnDownloadSample_Click" />
            <asp:Literal ID="litUploadStatus" runat="server" />
        </div>

        <!-- Summary & download failed -->
        <div class="upload-summary-box">
            <div class="summary-left">
                <!-- left summary area (optional values) -->
            </div>

            <div class="summary-right">
                <!-- THIS literal will show summary (Inserted / Failed / Download link etc.) -->
                <asp:Literal ID="litExcelSummary" runat="server" />
            </div>
        </div>

        <!-- ADD: minimal fail container used by popup (do not remove) -->
        <div id="divFailAlert" runat="server" style="display:none; margin-bottom:10px;">
            <asp:Literal ID="litFailAlert" runat="server" />
        </div>

        <!-- Search -->
        <div style="display:flex; gap:10px; margin:18px 0;">
            <asp:TextBox ID="txtSearch" runat="server" placeholder="Search by Name, Email, Gender, Country" style="padding:10px; border-radius:8px; border:1px solid #ccc; width:320px;"></asp:TextBox>
            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
            <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" />
        </div>

        <!-- Grid -->
        <div class="table-responsive">
            <asp:GridView ID="gvUsers" runat="server"
                AutoGenerateColumns="False"
                DataKeyNames="UserID"
                AllowPaging="true" PageSize="10"
                AllowSorting="true"
                CssClass="table"
                PagerStyle-CssClass="grid-pager"
                OnPageIndexChanging="gvUsers_PageIndexChanging"
                OnSorting="gvUsers_Sorting"
                OnRowCommand="gvUsers_RowCommand"
                OnRowDataBound="gvUsers_RowDataBound"
                OnRowCreated="gvUsers_RowCreated"
                ShowFooter="false">

                <Columns>
                    <asp:BoundField DataField="UserID" HeaderText="ID" SortExpression="UserID" HeaderStyle-Width="60px" />
                    <asp:TemplateField HeaderText="Name" SortExpression="FirstName">
                        <ItemTemplate><%# (Eval("FirstName") + " " + Eval("LastName")).ToString().Trim() %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                    <asp:BoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />
                    <asp:BoundField DataField="DOB" HeaderText="DOB" DataFormatString="{0:yyyy-MM-dd}" SortExpression="DOB" />
                    <asp:BoundField DataField="Country" HeaderText="Country" SortExpression="Country" />
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="action-buttons">
                                <asp:HyperLink ID="hlView" runat="server" Text="View" CssClass="btn btn-info" NavigateUrl='<%# ResolveUrl("~/Register%20Page/ViewUser.aspx?id=" + Eval("UserID")) %>' />
                                <asp:HyperLink ID="hlEdit" runat="server" Text="Edit" CssClass="btn btn-warning" NavigateUrl='<%# ResolveUrl("~/Register%20Page/EditUser.aspx?id=" + Eval("UserID")) %>' />
                                <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-danger" CommandName="del" CommandArgument='<%# Eval("UserID") %>' OnClientClick="return confirm('⚠️ Do you really want to delete this user?');" />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>

            </asp:GridView>
        </div>

        <asp:Label ID="lblMsg" runat="server" ForeColor="Red"></asp:Label>

    </div>
</asp:Content>
