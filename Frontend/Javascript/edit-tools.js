// Get tool ID from URL
const urlParams = new URLSearchParams(window.location.search);
const toolId = urlParams.get("id");

// Elements
const nameInput = document.getElementById("toolName");
const brandInput = document.getElementById("toolBrand");
const categoryInput = document.getElementById("toolCategory");
const imageInput = document.getElementById("toolImage");

// Load tool when page opens
document.addEventListener("DOMContentLoaded", loadTool);

async function loadTool() {
    const res = await fetch(`/api/tools/${toolId}`);
    const tool = await res.json();

    // Fill the form
    nameInput.value = tool.name;
    brandInput.value = tool.brand;
    categoryInput.value = tool.category;
    imageInput.value = tool.image ?? "";
}

// Save changes
document.getElementById("editToolForm").addEventListener("submit", async (e) => {
    e.preventDefault();

    const updatedTool = {
        name: nameInput.value,
        brand: brandInput.value,
        category: categoryInput.value,
        image: imageInput.value
    };

    await fetch(`/api/tools/${toolId}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(updatedTool)
    });

    alert("Tool updated!");
    window.location.href = "/admin/tool-management.html";
});
app.get("/api/tools/:id", async (req, res) => {
    const id = new ObjectId(req.params.id);
    const tool = await db.collection("tools").findOne({ _id: id });
    res.json(tool);
});
app.put("/api/tools/:id", async (req, res) => {
    const id = new ObjectId(req.params.id);

    const updateData = {
        name: req.body.name,
        brand: req.body.brand,
        category: req.body.category,
        image: req.body.image
    };

    await db.collection("tools").updateOne({ _id: id }, { $set: updateData });

    res.json({ success: true });
});