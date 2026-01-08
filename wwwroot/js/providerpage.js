document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".output-item").forEach(function (outputItem) {
        outputItem.addEventListener("click", function () {
            var name = this.getAttribute("data-name");
            var email = this.getAttribute("data-email");
            var description = this.getAttribute("data-description");
            var city = this.getAttribute("data-city");
            var state = this.getAttribute("data-state");
            var zipcode = this.getAttribute("data-zipcode");
            var service = this.getAttribute("data-service");
            var customerid = this.getAttribute("data-customer-id"); // NEW LINE

            console.log("Clicked output-item, customerId =", customerid);

            var container = document.getElementById("describe-container");
            container.innerHTML = "";

            var newCard = document.createElement("div");
            newCard.classList.add("describe-card");

            newCard.innerHTML = `
                <div class="row align-items-center">
                    <div class="col">
                        <h3 class="describe-title">${name}</h3>
                    </div>
                    <div class="col-auto">
                        <button class="btn btn-primary service-btn" data-customer-id="${customerid}">Provide Service</button>
                    </div>
                </div>
                <p><strong>Requested Service:</strong> ${service}</p>
                <p class="describe-city"><strong>City:</strong> ${city || "N/A"}</p>
                <p class="describe-state"><strong>State:</strong> ${state || "N/A"}</p>
                <p><strong>Zip Code:</strong> ${zipcode}</p>
                <p><strong>Description:</strong> ${description}</p>
                <p><strong>Contact:</strong> ${email}</p>
            `;

            container.appendChild(newCard);
        });
    });
});

document.addEventListener("click", function (e) {
    if (e.target.classList.contains("service-btn")) {
        const customerid = e.target.dataset.customerId;

        if (!customerid) {
            console.error("customerId is missing!");
            return;
        }
        
        console.log("Sending customerId to server:", customerid);

        fetch("/Home/ProvideService", {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },
            body: new URLSearchParams({
                customerId: String(customerid)
            })
        })
        .then(async response => {
            if (response.ok) {
                alert("Service offered!");
            } else {
                alert(message); // ← THIS is your BadRequest message
            }
        })
        .catch(err => {
            console.error("NETWORK / JS ERROR:", err);
            alert("Offer failed.");
        });

    }
});
