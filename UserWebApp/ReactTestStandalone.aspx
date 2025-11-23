<%@ Page Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.Page" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>React Test Standalone</title>

    <!-- React + Babel -->
    <script src="https://unpkg.com/react@18/umd/react.development.js"></script>
    <script src="https://unpkg.com/react-dom@18/umd/react-dom.development.js"></script>
    <script src="https://unpkg.com/babel-standalone@6/babel.min.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div id="react-root" style="margin:30px;"></div>

        <!-- Inline test -->
        <script type="text/babel">
            function InlineCheck() {
                const [n, setN] = React.useState(0);
                return (
                    <div style={{padding:20, border:'1px solid #ddd', width:420}}>
                        <h3>Inline React Test ✅</h3>
                        <p>Count: {n}</p>
                        <button onClick={() => setN(n + 1)}>Increase</button>
                    </div>
                );
            }
            ReactDOM.render(<InlineCheck />, document.getElementById('react-root'));
        </script>
    </form>
</body>
</html>
