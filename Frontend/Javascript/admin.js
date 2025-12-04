const API = "https://localhost:5001";  // Your backend URL

document.addEventListener("DOMContentLoaded", loadUsers);

async function loadUsers() {
    if (localStorage.getItem("IsAdmin") !== "true") {
        alert("You are not an admin.");
        window.location.href = "index.html";
        return;
    }

    const res = await fetch(`${API}/api/admin/users`, {
        credentials: "include"
    });

    const users = await res.json();

    const tableBody = document.querySelector("#userTable tbody");
    tableBody.innerHTML = "";

    users.forEach(user => {
        tableBody.innerHTML += `
            <tr>
                <td>${user.Username}</td>
                <td>${user.Email}</td>
                <td>${user.IsVerified}</td>
                <td>${user.IsAdmin}</td>
                <td>
                    <button class="action promote" onclick="promote('${user.Username}')">Promote</button>
                    <button class="action delete" onclick="removeUser('${user.Username}')">Delete</button>
                    <button class="action reset" onclick="resetPassword('${user.Username}')">Reset PW</button>
                </td>
            </tr>
        `;
    });
}

async function promote(username) {
    await fetch(`${API}/api/admin/promote/${username}`, {
        method: "PUT",
        credentials: "include"
    });
    loadUsers();
}

async function removeUser(username) {
    await fetch(`${API}/api/admin/delete/${username}`, {
        method: "DELETE",
        credentials: "include"
    });
    loadUsers();
}

async function resetPassword(username) {
    await fetch(`${API}/api/admin/reset/${username}`, {
        method: "PUT",
        credentials: "include"
    });
    alert("Password reset to: temporary123");
}

document.getElementById("addToolForm").addEventListener("submit", async (e) => {
    e.preventDefault();

    const toolData = {
        name: document.getElementById("name").value,
        brand: document.getElementById("brand").value,
        category: document.getElementById("category").value,
        description: document.getElementById("description").value,
        imageUrl: document.getElementById("image").value
    };

    try {
        const response = await fetch("https://localhost:5000/api/tools/add", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(toolData)
        });

        const result = await response.json();
        
        if (response.ok) {
            alert("Tool added successfully!");
            window.location.href = "/admin.html";
        } else {
            alert("Error: " + result.message);
        }

    } catch (err) {
        alert("Failed to connect to server.");
    }
});