(function () {
    "use strict";

    let cleanup = null;

    window.site = {
        prefersReducedMotion: function () {
            return window.matchMedia("(prefers-reduced-motion: reduce)").matches;
        },

        printPage: function () {
            window.print();
        },

        // Reports the section currently under the sticky header back to .NET.
        // Active = last section whose top has passed 45% of the viewport height.
        observeSections: function (dotNetRef, ids) {
            window.site.disconnectSections();
            const sections = ids
                .map(function (id) { return document.getElementById(id); })
                .filter(function (el) { return el !== null; });
            if (sections.length === 0) {
                return;
            }

            let last;
            let pending = false;

            function update() {
                pending = false;
                const line = window.innerHeight * 0.45;
                let active = null;
                sections.forEach(function (el) {
                    if (el.getBoundingClientRect().top <= line) {
                        active = el.id;
                    }
                });
                if (window.innerHeight + window.scrollY >= document.documentElement.scrollHeight - 2) {
                    active = sections[sections.length - 1].id;
                }
                if (active !== last) {
                    last = active;
                    dotNetRef.invokeMethodAsync("SetActiveSection", active);
                }
            }

            function schedule() {
                if (!pending) {
                    pending = true;
                    window.requestAnimationFrame(update);
                }
            }

            window.addEventListener("scroll", schedule, { passive: true });
            window.addEventListener("resize", schedule, { passive: true });
            cleanup = function () {
                window.removeEventListener("scroll", schedule);
                window.removeEventListener("resize", schedule);
            };
            update();
        },

        disconnectSections: function () {
            if (cleanup) {
                cleanup();
                cleanup = null;
            }
        }
    };
})();
