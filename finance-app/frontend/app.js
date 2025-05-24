async function getUsers() {
  const response = await fetch("http://localhost:5000/api/employee");
  const users = await response.json();
  const list = document.getElementById("userList");
  list.innerHTML = "";
  users.forEach(u => {
    const li = document.createElement("li");
    li.innerText = `${u.employeeNumber} - ${u.gender} - ${u.department}`;
    list.appendChild(li);
  });
}