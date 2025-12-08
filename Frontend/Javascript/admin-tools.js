const API = "http://localhost:5000/api/tools";

async function loadTools() {
    try {
        const response = await fetch(API);
        const tools = await response.json();

        const tableBody = document.getElementById("toolTableBody");
        tableBody.innerHTML = "";

        tools.forEach(tool => {
            const row = document.createElement("tr");

            row.innerHTML = `
                <td>${tool.name}</td>
                <td>${tool.brand}</td>
                <td>${tool.category}</td>
                <td><img src="${tool.image}" width="60" /></td>
                <td>
                    <button class="btn-delete" onclick="deleteTool('${tool.id}')">Delete</button>
                </td>
            `;

            tableBody.appendChild(row);
        });

    } catch (err) {
        console.error("Error loading tools:", err);
    }
}

async function deleteTool(id) {
    if (!confirm("Are you sure you want to delete this tool?")) return;

    await fetch(`${API}/${id}`, {
        method: "DELETE",
    });

    loadTools();
}

document.addEventListener("DOMContentLoaded", loadTools);