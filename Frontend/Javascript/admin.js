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