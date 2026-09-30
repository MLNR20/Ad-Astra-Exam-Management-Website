// Lets users jump to a page from the navbar by name, matching against the
// list of web pages stored in the database (fetched via the WebPages API).
(function () {
    document.addEventListener('DOMContentLoaded', function () {
        var input = document.getElementById('navbarSearch');
        var resultsBox = document.getElementById('navbarSearchResults');
        if (!input || !resultsBox) return;

        var pages = [];

        fetch('/api/v1/WebPages')
            .then(function (res) { return res.ok ? res.json() : []; })
            .then(function (data) {
                pages = data.map(function (page) {
                    return { name: page.name, href: page.url };
                });
            })
            .catch(function () { pages = []; });

        function renderResults(matches) {
            resultsBox.innerHTML = '';

            if (matches.length === 0) {
                resultsBox.classList.add('d-none');
                return;
            }

            matches.forEach(function (page) {
                var item = document.createElement('a');
                item.href = page.href;
                item.className = 'list-group-item list-group-item-action';
                item.textContent = page.name;
                resultsBox.appendChild(item);
            });
            resultsBox.classList.remove('d-none');
        }

        function search() {
            var term = input.value.trim().toLowerCase();
            if (!term) {
                // Nothing typed yet: show the full list of pages from the database.
                renderResults(pages);
                return;
            }
            renderResults(pages.filter(function (page) {
                return page.name.toLowerCase().indexOf(term) !== -1;
            }));
        }

        input.addEventListener('input', search);
        input.addEventListener('focus', search);

        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                var firstMatch = resultsBox.querySelector('.list-group-item');
                if (firstMatch) {
                    e.preventDefault();
                    window.location.href = firstMatch.getAttribute('href');
                }
            } else if (e.key === 'Escape') {
                renderResults([]);
            }
        });

        document.addEventListener('click', function (e) {
            if (e.target !== input && !resultsBox.contains(e.target)) {
                renderResults([]);
            }
        });
    });
})();
