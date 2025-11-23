// Scripts/ReactDemo.js
console.log("ReactDemo.js loaded");

function LiveSearchDemo() {
  const items = [
    { id: 1, name: "Apple", info: "Fresh red apple" },
    { id: 2, name: "Banana", info: "Yellow banana" },
    { id: 3, name: "Orange", info: "Citrus orange" },
    { id: 4, name: "Mango", info: "King of fruits" },
    { id: 5, name: "Pineapple", info: "Tropical fruit" }
  ];

  const [query, setQuery] = React.useState("");
  const filtered = items.filter(i => i.name.toLowerCase().includes(query.toLowerCase()));

  return (
    <div style={{ maxWidth: 680, margin: "30px auto", fontFamily: "Arial, sans-serif" }}>
      <h2 style={{ marginBottom: 6 }}>React Live Search Demo</h2>
      <input
        type="text"
        placeholder="Search..."
        value={query}
        onChange={e => setQuery(e.target.value)}
        style={{ width: "100%", padding: "10px", borderRadius: 6, border: "1px solid #ccc", marginBottom: 14 }}
      />
      <div style={{ display: "grid", gridTemplateColumns:"repeat(auto-fit, minmax(220px, 1fr))", gap: 12 }}>
        {filtered.length === 0 && <div style={{ padding: 14, color: "#777" }}>No items found.</div>}
        {filtered.map(item => (
          <div key={item.id} style={{ padding: 12, borderRadius: 8, boxShadow:"0 2px 8px rgba(0,0,0,0.08)", background:"#fff" }}>
            <h3 style={{ margin:"0 0 6px 0" }}>{item.name}</h3>
            <p style={{ margin:0, color:"#555" }}>{item.info}</p>
          </div>
        ))}
      </div>
    </div>
  );
}

ReactDOM.render(<LiveSearchDemo />, document.getElementById("react-root"));
