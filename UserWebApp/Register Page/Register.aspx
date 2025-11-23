<%@ Page Language="C#" Async="true" AutoEventWireup="true"
    CodeBehind="Register.aspx.cs"
    Inherits="UserWebApp.RegisterPage.Register"
    MasterPageFile="~/Site.Master" %>

<asp:Content ID="RegistrationContent" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Classical / Elegant CSS for registration form -->
    <style>
        /* page wrapper */
        .reg-wrapper {
            background: linear-gradient(180deg, #fcfbf8 0%, #f7f3ee 100%);
            padding: 30px;
            border-radius: 14px;
            box-shadow: 0 10px 30px rgba(41, 41, 41, 0.06);
            max-width: 920px;
            margin: 18px auto;
            border: 1px solid rgba(100,80,70,0.08);
            font-family: Georgia, "Times New Roman", serif;
            color: #2b2b2b;
        }

        /* heading */
        #lblUserRegistration {
            font-size: 26px;
            color: #3b2f2f;
            font-weight: 700;
            margin-bottom: 12px;
            display: inline-block;
            padding-bottom: 6px;
            border-bottom: 2px solid rgba(59,47,47,0.06);
        }

        /* table cell spacing */
        #tblUserRegistration td {
            padding: 12px 8px;
            vertical-align: middle;
        }

        /* left labels */
        #tblUserRegistration label,
        #tblUserRegistration .form-label {
            font-weight: 700;
            color: #3b2f2f;
            font-size: 15px;
            letter-spacing: 0.2px;
        }

        /* inputs */
        #tblUserRegistration input[type="text"],
        #tblUserRegistration input[type="password"],
        #tblUserRegistration input[type="date"],
        #tblUserRegistration textarea,
        #tblUserRegistration select {
            padding: 10px 12px;
            width: 320px;
            border-radius: 8px;
            border: 1px solid rgba(75,63,63,0.12);
            background: #fffefb;
            color: #222;
            box-sizing: border-box;
            box-shadow: inset 0 1px 0 rgba(255,255,255,0.6);
            transition: box-shadow .15s ease, border-color .15s ease, transform .05s ease;
            font-family: Georgia, "Times New Roman", serif;
            font-size: 14px;
        }

        /* textarea smaller height tweak */
        #tblUserRegistration textarea { min-height: 80px; max-width:520px; }

        /* focus */
        #tblUserRegistration input:focus,
        #tblUserRegistration textarea:focus,
        #tblUserRegistration select:focus {
            border-color: rgba(90,45,45,0.55);
            box-shadow: 0 6px 18px rgba(59,47,47,0.06);
            outline: none;
            transform: translateY(-1px);
        }

        /* small helper text (validators) */
        .text-danger, span[style*="color:Red"] {
            color: #b22222 !important;
            font-size: 13px;
        }

        /* buttons - classical maroon style */
        .reg-actions {
            margin-top: 8px;
        }

        #btnRegister {
            background: linear-gradient(180deg,#6b2e2e 0%, #5b2525 100%);
            color: #fff;
            border: 1px solid rgba(0,0,0,0.08);
            padding: 9px 16px;
            border-radius: 8px;
            font-weight: 700;
            box-shadow: 0 6px 14px rgba(107,46,46,0.12);
            cursor: pointer;
            font-family: Georgia, "Times New Roman", serif;
        }

        #btnRegister:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 22px rgba(107,46,46,0.14);
        }

        #btnReset {
            background: #fff;
            color: #3b2f2f;
            border: 1px solid rgba(75,63,63,0.12);
            padding: 8px 14px;
            border-radius: 8px;
            margin-left: 10px;
            cursor: pointer;
            font-weight: 600;
        }

        /* file upload style small tweak */
        #tblUserRegistration input[type="file"] {
            padding: 6px 8px;
            font-family: inherit;
        }

        /* responsive */
        @media(max-width:820px) {
            #tblUserRegistration input,
            #tblUserRegistration textarea,
            #tblUserRegistration select { width: 100% !important; max-width:100%; }
            .reg-wrapper { padding: 20px; margin: 12px; }
        }

    </style>

    <div class="reg-wrapper">

        <asp:ValidationSummary ID="valSummary" runat="server" CssClass="text-danger"
            HeaderText="Please fix the following errors:"
            DisplayMode="BulletList" ShowSummary="true"
            ValidationGroup="Reg" />

        <asp:Panel ID="pnlUserRegistration" runat="server" Width="100%">
            <asp:Table ID="tblUserRegistration" runat="server" Width="100%">

                <asp:TableRow>
                    <asp:TableCell ColumnSpan="2">
                        <asp:Label ID="lblUserRegistration" runat="server"
                            Text="New User Registration" Font-Bold="True" />
                    </asp:TableCell>
                </asp:TableRow>

                <%-- First Name --%>
                <asp:TableRow>
                    <asp:TableCell Width="20%">
                        <asp:Label ID="lblFirstName" runat="server" Text="First Name" />
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="txtFirst" runat="server" MaxLength="100" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFirst"
                            ErrorMessage="First name is required" ForeColor="Red"
                            Display="Dynamic" ValidationGroup="Reg" />
                    </asp:TableCell>
                </asp:TableRow>

                <%-- Last Name --%>
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="lblLastName" runat="server" Text="Last Name" />
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="txtLast" runat="server" MaxLength="100" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtLast"
                            ErrorMessage="Last name is required" ForeColor="Red"
                            Display="Dynamic" ValidationGroup="Reg" />
                    </asp:TableCell>
                </asp:TableRow>

                <%-- Email --%>
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="lblEmail" runat="server" Text="Email" />
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="txtEmail" runat="server" MaxLength="150" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail"
                            ErrorMessage="Email is required" ForeColor="Red"
                            Display="Dynamic" ValidationGroup="Reg" />
                        <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmail"
                            ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$"
                            ErrorMessage="Please enter a valid email address"
                            ForeColor="Red" Display="Dynamic" ValidationGroup="Reg" />
                    </asp:TableCell>
                </asp:TableRow>

                <%-- Password --%>
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="lblPassword" runat="server" Text="Password" />
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" MaxLength="100" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPassword"
                            ErrorMessage="Password is required" ForeColor="Red"
                            Display="Dynamic" ValidationGroup="Reg" />
                        <asp:RegularExpressionValidator runat="server" ControlToValidate="txtPassword"
                            ValidationExpression="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$"
                            ErrorMessage="Password must be 8+ chars with upper, lower, number & special char"
                            ForeColor="Red" Display="Dynamic" ValidationGroup="Reg" />
                    </asp:TableCell>
                </asp:TableRow>

                <%-- Confirm Password --%>
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="lblConfirm" runat="server" Text="Confirm Password" />
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="txtConfirm" runat="server" TextMode="Password" MaxLength="100" />
                        <asp:CompareValidator runat="server" ControlToValidate="txtConfirm"
                            ControlToCompare="txtPassword" ErrorMessage="Passwords do not match"
                            ForeColor="Red" Display="Dynamic" ValidationGroup="Reg" />
                    </asp:TableCell>
                </asp:TableRow>

                <%-- Gender --%>
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="lblGender" runat="server" Text="Gender" />
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:RadioButtonList ID="rblGender" runat="server" RepeatDirection="Horizontal">
                            <asp:ListItem Text="Male" Value="Male" />
                            <asp:ListItem Text="Female" Value="Female" />
                        </asp:RadioButtonList>
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="rblGender"
                            ErrorMessage="Please select gender" ForeColor="Red"
                            Display="Dynamic" ValidationGroup="Reg" />
                    </asp:TableCell>
                </asp:TableRow>

                <%-- DOB (>= 18) --%>
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="lblDob" runat="server" Text="Date of Birth (YYYY-MM-DD)" />
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="txtDob" runat="server" TextMode="Date" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtDob"
                            ErrorMessage="Please select your date of birth" ForeColor="Red"
                            Display="Dynamic" ValidationGroup="Reg" />
                        <asp:CustomValidator ID="cvDob18" runat="server"
                            ControlToValidate="txtDob" ClientValidationFunction="validateAge18"
                            OnServerValidate="cvDob18_ServerValidate"
                            ErrorMessage="You must be at least 18 years old"
                            ForeColor="Red" Display="Dynamic" ValidationGroup="Reg" />
                    </asp:TableCell>
                </asp:TableRow>

                <%-- Country (from API) --%>
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="lblCountry" runat="server" Text="Country" />
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:DropDownList ID="ddlCountry" runat="server" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlCountry_SelectedIndexChanged">
                            <asp:ListItem Text="Select Country" Value="0" />
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator runat="server"
                            ControlToValidate="ddlCountry" InitialValue="0"
                            ErrorMessage="Please select a country" ForeColor="Red"
                            Display="Dynamic" ValidationGroup="Reg" />
                    </asp:TableCell>
                </asp:TableRow>

                <%-- State (from API by country) --%>
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="Statetxt" runat="server" Text="State" />
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:DropDownList ID="DropDownState" runat="server" AutoPostBack="true"
                            OnSelectedIndexChanged="DropDownState_SelectedIndexChanged">
                            <asp:ListItem Text="Select State" Value="0" />
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator runat="server"
                            ControlToValidate="DropDownState" InitialValue="0"
                            ErrorMessage="Please select a state" ForeColor="Red"
                            Display="Dynamic" ValidationGroup="Reg" />
                    </asp:TableCell>
                </asp:TableRow>

                <%-- City (from API by state) --%>
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="Citytext" runat="server" Text="City" />
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:DropDownList ID="DropDownCity" runat="server">
                            <asp:ListItem Text="Select City" Value="0" />
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator runat="server"
                            ControlToValidate="DropDownCity" InitialValue="0"
                            ErrorMessage="Please select a city" ForeColor="Red"
                            Display="Dynamic" ValidationGroup="Reg" />
                    </asp:TableCell>
                </asp:TableRow>

                <%-- Address --%>
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="lblAddress" runat="server" Text="Address" />
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:TextBox ID="txtAddress" runat="server" TextMode="MultiLine"
                            Rows="3" MaxLength="200" />
                        <asp:RequiredFieldValidator runat="server"
                            ControlToValidate="txtAddress" ErrorMessage="Address is required"
                            ForeColor="Red" Display="Dynamic" ValidationGroup="Reg" />
                    </asp:TableCell>
                </asp:TableRow>

                <%-- Terms (CustomValidator WITHOUT ControlToValidate) --%>
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="lblTerms" runat="server" Text="Accept Terms" />
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:CheckBox ID="chkTerms" runat="server"
                            Text="I agree to Terms & Conditions" />
                        <asp:CustomValidator ID="cvTerms" runat="server"
                            ValidateEmptyText="true" ClientValidationFunction="validateTerms"
                            OnServerValidate="cvTerms_ServerValidate"
                            ErrorMessage="You must accept Terms & Conditions"
                            ForeColor="Red" Display="Dynamic" ValidationGroup="Reg" />
                    </asp:TableCell>
                </asp:TableRow>

                <%-- Profile Image (type check in code-behind) --%>
                <asp:TableRow>
                    <asp:TableCell>
                        <asp:Label ID="lblProfile" runat="server" Text="Profile Image" />
                    </asp:TableCell>
                    <asp:TableCell>
                        <asp:FileUpload ID="fuProfile" runat="server" />
                    </asp:TableCell>
                </asp:TableRow>

                <%-- Buttons --%>
                <asp:TableRow>
                    <asp:TableCell ColumnSpan="2">
                        <div class="reg-actions">
                            <asp:Button ID="btnRegister" runat="server" Text="Register"
                                ToolTip="Click to Register" OnClick="btnRegister_Click"
                                CausesValidation="true" ValidationGroup="Reg" />

                            <asp:Button ID="btnReset" runat="server" Text="Reset"
                                ToolTip="Reset form" OnClick="btnReset_Click"
                                CausesValidation="false" />
                        </div>
                    </asp:TableCell>
                </asp:TableRow>

                <asp:TableRow>
                    <asp:TableCell ColumnSpan="2">
                        <asp:Label ID="lblResult" runat="server" ForeColor="Red" />
                    </asp:TableCell>
                </asp:TableRow>

            </asp:Table>
        </asp:Panel>
    </div>

    <!-- Client-side validators (unchanged) -->
    <script type="text/javascript">
        (function () {
            var dob = document.getElementById('<%= txtDob.ClientID %>');
            if (dob) {
                var d = new Date();
                d.setFullYear(d.getFullYear() - 18);
                var yyyy = d.getFullYear();
                var mm = ('0' + (d.getMonth() + 1)).slice(-2);
                var dd = ('0' + d.getDate()).slice(-2);
                dob.setAttribute('max', yyyy + '-' + mm + '-' + dd);
            }
        })();

        function validateAge18(sender, args) {
            var val = args.Value;
            if (!val) { args.IsValid = false; return; }
            var dob = new Date(val);
            if (isNaN(dob.getTime())) { args.IsValid = false; return; }
            var now = new Date();
            var eighteen = new Date(now.getFullYear() - 18, now.getMonth(), now.getDate());
            args.IsValid = (dob <= eighteen);
        }

        function validateTerms(sender, args) {
            var chk = document.getElementById('<%= chkTerms.ClientID %>');
            args.IsValid = chk && chk.checked;
        }
    </script>

</asp:Content>
