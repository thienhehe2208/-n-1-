// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
    const sidebarToggle = document.getElementById("sidebarToggle");

    if (sidebarToggle) {
        sidebarToggle.addEventListener("click", function () {
            if (window.innerWidth <= 820) {
                document.body.classList.toggle("mobile-sidebar-open");
            } else {
                document.body.classList.toggle("sidebar-collapsed");
            }
        });

        document.addEventListener("click", function (event) {
            const sidebar = document.getElementById("adminSidebar");

            if (
                window.innerWidth <= 820 &&
                document.body.classList.contains("mobile-sidebar-open") &&
                sidebar &&
                !sidebar.contains(event.target) &&
                !sidebarToggle.contains(event.target)
            ) {
                document.body.classList.remove("mobile-sidebar-open");
            }
        });
    }

    document.querySelectorAll("[data-book-search]").forEach(setupBookSuggestions);
});

function setupBookSuggestions(form) {
    const input = form.querySelector("[data-book-search-input]");
    const results = form.querySelector("[data-book-search-results]");
    const endpoint = form.dataset.suggestionUrl;
    const detailsBase = endpoint?.replace(/\/GoiYTimKiem\/?$/i, "/Details/");
    let timer = null;
    let request = null;
    let activeIndex = -1;

    if (!input || !results || !endpoint) return;

    const close = function () {
        results.classList.remove("is-open");
        input.setAttribute("aria-expanded", "false");
        activeIndex = -1;
    };

    const open = function () {
        results.classList.add("is-open");
        input.setAttribute("aria-expanded", "true");
    };

    const showMessage = function (icon, title, message) {
        results.replaceChildren();
        const box = document.createElement("div");
        box.className = "book-suggestion-message";
        const iconElement = document.createElement("i");
        iconElement.className = `bi ${icon}`;
        const copy = document.createElement("span");
        const strong = document.createElement("strong");
        strong.textContent = title;
        const small = document.createElement("small");
        small.textContent = message;
        copy.append(strong, small);
        box.append(iconElement, copy);
        results.append(box);
        open();
    };

    const render = function (books, keyword) {
        results.replaceChildren();
        activeIndex = -1;

        const normalizedKeyword = keyword.toLocaleLowerCase("vi");
        const authors = Array.from(new Set(
            books.flatMap(book => Array.isArray(book.tacGias) ? book.tacGias : [])
                .filter(author => author && author.toLocaleLowerCase("vi").includes(normalizedKeyword))
        ));

        if (authors.length) {
            const authorHeading = document.createElement("div");
            authorHeading.className = "book-suggestion-heading";
            authorHeading.textContent = "Tác giả phù hợp";
            results.append(authorHeading);

            authors.forEach(function (authorName) {
                const authorButton = document.createElement("button");
                authorButton.type = "button";
                authorButton.className = "book-author-suggestion";
                authorButton.setAttribute("role", "option");

                const icon = document.createElement("i");
                icon.className = "bi bi-person";
                const name = document.createElement("span");
                name.textContent = authorName;
                const arrow = document.createElement("i");
                arrow.className = "bi bi-arrow-return-left book-suggestion-arrow";
                authorButton.append(icon, name, arrow);

                authorButton.addEventListener("click", function () {
                    input.value = authorName;
                    close();
                    form.requestSubmit();
                });
                results.append(authorButton);
            });
        }

        const heading = document.createElement("div");
        heading.className = "book-suggestion-heading";
        heading.textContent = books.length ? "Sách phù hợp" : "Kết quả tìm kiếm";
        results.append(heading);

        if (!books.length) {
            showMessage("bi-search", "Không tìm thấy sách", `Không có kết quả phù hợp với “${keyword}”.`);
            return;
        }

        books.forEach(function (book) {
            const link = document.createElement("a");
            link.className = "book-suggestion-item";
            link.href = `${detailsBase}${book.maSach}`;
            link.setAttribute("role", "option");

            const cover = document.createElement("span");
            cover.className = "book-suggestion-cover";
            const placeholder = document.createElement("i");
            placeholder.className = "bi bi-book";
            cover.append(placeholder);
            if (book.anhBia) {
                const image = document.createElement("img");
                image.src = book.anhBia;
                image.alt = "";
                image.addEventListener("error", function () { image.remove(); });
                cover.append(image);
            }

            const copy = document.createElement("span");
            copy.className = "book-suggestion-copy";
            const title = document.createElement("strong");
            title.textContent = book.tenSach;
            const author = document.createElement("span");
            author.textContent = book.tacGia || "Chưa cập nhật tác giả";
            const meta = document.createElement("small");
            meta.textContent = `${book.theLoai} · ISBN ${book.isbn || "--"}`;
            copy.append(title, author, meta);

            const arrow = document.createElement("i");
            arrow.className = "bi bi-chevron-right book-suggestion-arrow";
            link.append(cover, copy, arrow);
            results.append(link);
        });

        const hint = document.createElement("div");
        hint.className = "book-suggestion-hint";
        hint.textContent = "Nhấn Enter để xem tất cả kết quả";
        results.append(hint);
        open();
    };

    const search = async function () {
        const keyword = input.value.trim();
        if (keyword.length < 2) {
            close();
            results.replaceChildren();
            return;
        }

        request?.abort();
        request = new AbortController();
        showMessage("bi-hourglass-split", "Đang tìm kiếm", "Vui lòng chờ trong giây lát...");

        try {
            const response = await fetch(`${endpoint}?q=${encodeURIComponent(keyword)}`, {
                signal: request.signal,
                headers: { "X-Requested-With": "XMLHttpRequest" }
            });
            if (!response.ok) throw new Error("Suggestion request failed");
            render(await response.json(), keyword);
        } catch (error) {
            if (error.name !== "AbortError") {
                showMessage("bi-wifi-off", "Chưa thể tải gợi ý", "Bạn vẫn có thể nhấn Enter để tìm kiếm.");
            }
        }
    };

    input.addEventListener("input", function () {
        clearTimeout(timer);
        timer = setTimeout(search, 220);
    });

    input.addEventListener("focus", function () {
        if (input.value.trim().length >= 2 && results.childElementCount) open();
    });

    input.addEventListener("keydown", function (event) {
        const items = Array.from(results.querySelectorAll(".book-author-suggestion, .book-suggestion-item"));
        if (event.key === "Escape") {
            close();
            return;
        }
        if (event.key === "Enter" && activeIndex >= 0 && items[activeIndex]) {
            event.preventDefault();
            items[activeIndex].click();
            return;
        }
        if (!items.length || !["ArrowDown", "ArrowUp"].includes(event.key)) return;
        event.preventDefault();
        activeIndex = event.key === "ArrowDown"
            ? (activeIndex + 1) % items.length
            : (activeIndex - 1 + items.length) % items.length;
        items.forEach((item, index) => item.classList.toggle("is-active", index === activeIndex));
        items[activeIndex].scrollIntoView({ block: "nearest" });
    });

    document.addEventListener("click", function (event) {
        if (!form.contains(event.target)) close();
    });
}
