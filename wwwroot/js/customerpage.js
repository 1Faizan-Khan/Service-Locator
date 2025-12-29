document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".output-item").forEach(function (outputItem) {
        outputItem.addEventListener("click", function () {
            var itemname = this.querySelector(".itemname").textContent;
            var professionname = this.querySelector(".professionName").textContent;
            var description = this.getAttribute("data-description");
            var city = this.getAttribute("data-city");
            var state = this.getAttribute("data-state");
            var zipcode = this.getAttribute("data-zipcode");
            var providerId = this.getAttribute("data-provider-id"); // NEW LINE

            var container = document.getElementById("describe-container");

            // Clear previous content
            container.innerHTML = "";

            // Create a new div for the clicked post
            var newCard = document.createElement("div");
            newCard.classList.add("describe-card");

            newCard.innerHTML = `
                <div class="row align-items-center">
                    <div class="col-auto">
                        <h3 class="describe-title">${itemname}</h3>
                    </div>
                    <div class="col-auto ms-3">
                        <button class="btn btn-primary request-btn" data-provider-id="${providerId}">Request</button>
                    </div>
                </div>
                <p class="describe-profession">${professionname}</p>
                <p class="describe-city"><strong>City:</strong> ${city || "N/A"}</p>
                <p class="describe-state"><strong>State:</strong> ${state || "N/A"}</p>
                <p class="describe-zipcode"><strong>Zip Code:</strong> ${zipcode || "N/A"}</p>
                <p class="describe-description">${description}</p>
            `;

            container.appendChild(newCard);
        });
    });
});

document.addEventListener("click", function (e) {
    if (e.target.classList.contains("request-btn")) {
        const providerId = e.target.dataset.providerId;

        fetch("/Home/RequestService", {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },
            body: `providerId=${providerId}`
        })
        .then(response => {
            if (response.ok) {
                // Optional: give visual feedback
                alert("Service requested!");
            } else {
                alert("You alraeady requested this service.");
            }
        })
        .catch(err => {
            console.error(err);
            alert("Request failed.");
        });
    }
});

