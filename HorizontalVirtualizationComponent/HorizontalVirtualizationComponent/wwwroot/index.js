window.scrollCarousel = (element, distance) => {
    element.scrollBy({ left: distance, behavior: "smooth" });
};
function calculateContainerWidth  (){
    var divWidth = window.innerWidth;
    var sidebar = document.querySelector(".sidebar");
    var widthOfSideBar = sidebar.getBoundingClientRect().width;
    divWidth = divWidth - widthOfSideBar;
    return divWidth;
};

function getScrollInfo(elementId) {
    var el = document.getElementById(elementId);
    console.log(el);
    console.log(el.scrollWidth);
    console.log(el.clientWidth);
    if (!el) {
        return { scrollLeft: 0, maxScroll: 0 };
    }
    return {
        scrollLeft: el.scrollLeft,
        // Compute max scroll using scrollWidth and clientWidth
        maxScroll: el.scrollWidth - el.clientWidth
    };
};

function setScrollLeft(containerId, newScrollLeft) {
    var container = document.getElementById(containerId);
    if (container) {
        container.scrollLeft = newScrollLeft;
    }
}