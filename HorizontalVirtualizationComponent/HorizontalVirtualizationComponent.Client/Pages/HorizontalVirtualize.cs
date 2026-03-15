using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace HorizontalVirtualizationComponent.Client.Pages
{
    public class HorizontalVirtualize<TItem> : ComponentBase
    {
        #region Parameters

        /// <summary>
        /// The complete collection of items to render.
        /// </summary>
        [Parameter] public ICollection<TItem>? Items { get; set; }

        /// <summary>
        /// The template to render each item.
        /// </summary>
        [Parameter] public RenderFragment<TItem>? ItemTemplate { get; set; }

        /// <summary>
        /// The width (in pixels) of the scrolling container.
        /// </summary>
        [Parameter] public double ContainerWidth { get; set; } = 500;

        /// <summary>
        /// The width (in pixels) of each item.
        /// </summary>
        [Parameter] public double ItemSize { get; set; } = 100;

        /// <summary>
        /// The number of extra items rendered on each side beyond the visible range.
        /// </summary>
        [Parameter] public int OverScanCount { get; set; } = 2;

        [Parameter] public int NavJumpItemCount { get; set; } = 1;
        #endregion

        #region Injections and Private Fields

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        // Generate a unique ID for the container so it can be located via JS.
        private readonly string _containerId = "simpleHorizontalVirtualize_" + Guid.NewGuid().ToString("N");

        // The index of the first item currently rendered.
        private int _firstIndex = 0;
        // The number of items that fit in the container.
        private int _visibleCount;
        // The last index (exclusive) of the rendered items.
        private int _lastIndex = 0;
        // Fields for tracking the drag/touch state.
        // Drag state for both mouse and touch.
        private double _startX;
        private double _scrollStartX;
        private bool _isDragging = false;
        private double scrollLeft = 0;
        private double maxScroll = 0;
        #endregion

        #region Initialization

        protected override void OnInitialized()
        {


            // Compute how many items can fit in the container.
            _visibleCount = (int)Math.Ceiling(ContainerWidth / ItemSize);
            // Initialize _lastIndex to include visible items plus overscan on both sides.
            _lastIndex = _firstIndex + _visibleCount + 2 * OverScanCount;

        }

        protected override void OnParametersSet()
        {
            if (ItemSize <= 0)
            {
                throw new InvalidOperationException(
                    $"{GetType()} requires a positive value for parameter '{nameof(ItemSize)}'.");
            }

            if (Items == null)
            {
                throw new InvalidOperationException(
                    $"{GetType()} requires either the '{nameof(Items)}'" +
                    $"and non-null.");

            }
        }
        #endregion

        #region Scroll Handling

        /// <summary>
        /// Called when the container is scrolled. Uses JS interop to determine the current scrollLeft.
        /// </summary>
        private async Task OnScrollAsync()
        {
            // Use a simple JS interop call to read the scrollLeft of the container.
            var scrollInfo = await JSRuntime.InvokeAsync<ScrollInfo>("getScrollInfo", _containerId);
            //maxScroll = await JSRuntime.InvokeAsync<double>("eval", $"document.getElementById('{_containerId}').maxScroll");
            maxScroll = scrollInfo.MaxScroll;
            scrollLeft = scrollInfo.ScrollLeft;
            // Calculate the new first index based on scroll position (with an extra overscan to the left).
            int newFirstIndex = (int)(scrollLeft / ItemSize) - OverScanCount;
            if (newFirstIndex < 0)
            {
                newFirstIndex = 0;
            }
            _firstIndex = newFirstIndex;
            _lastIndex = _firstIndex + _visibleCount + 2 * OverScanCount;
            StateHasChanged();
        }

        #endregion

        #region Render Tree

        protected override async void BuildRenderTree(RenderTreeBuilder builder)
        {
            // Container for navigation buttons and the horizontal scroll container.
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "style", "margin-bottom: 5px;");

            // Left button.
            if (scrollLeft > 0)
            {
                builder.OpenElement(2, "button");
                builder.AddAttribute(3, "onclick", EventCallback.Factory.Create(this, MoveLeftAsync));
                builder.AddContent(4, "Left");
                builder.CloseElement();
            }

            // Space between buttons.
            builder.AddContent(5, "");

            // Right button.
            if (scrollLeft != maxScroll)
            {
                builder.OpenElement(6, "button");
                builder.AddAttribute(7, "onclick", EventCallback.Factory.Create(this, MoveRightAsync));
                builder.AddContent(8, "Right");
                builder.CloseElement();
            }

            builder.CloseElement(); // End of button container.

            // Outer container div.
            builder.OpenElement(9, "div");
            builder.AddAttribute(10, "id", _containerId);
            builder.AddAttribute(11, "style", "overflow-x: auto; white-space: nowrap; position: relative; width: 100%; scroll-behavior: smooth; transition: 0.3s ease-in-out; cursor: -webkit-grab;");


            // Attach mouse events.
            builder.AddAttribute(12, "onmousedown", EventCallback.Factory.Create<MouseEventArgs>(this, OnMouseDown));
            builder.AddEventPreventDefaultAttribute(13, "onmousedown", true);
            builder.AddAttribute(14, "onmousemove", EventCallback.Factory.Create<MouseEventArgs>(this, OnMouseMove));
            builder.AddEventPreventDefaultAttribute(15, "onmousemove", true);
            builder.AddAttribute(16, "onmouseup", EventCallback.Factory.Create<MouseEventArgs>(this, OnMouseUp));
            builder.AddEventPreventDefaultAttribute(17, "onmouseup", true);
            builder.AddAttribute(18, "onmouseleave", EventCallback.Factory.Create<MouseEventArgs>(this, OnMouseUp)); // End dragging if mouse leaves.
            builder.AddAttribute(19, "onscroll", EventCallback.Factory.Create(this, OnScrollAsync));

            // Left spacer div: width equal to the space taken by items preceding the first rendered item.
            double leftSpacerWidth = _firstIndex * ItemSize;
            builder.OpenElement(20, "div");
            builder.AddAttribute(21, "style", $"display: inline-block; width: {leftSpacerWidth}px;");
            builder.CloseElement();

            // Render the visible items (plus an overscan on both sides).
            // Ensure we don't exceed the available items.
            var itemsToRender = Items!.Skip(_firstIndex)
                                      .Take(Math.Min(Items!.Count - _firstIndex, _visibleCount + 2 * OverScanCount));
            foreach (var item in itemsToRender)
            {
                builder.OpenElement(22, "div");
                builder.AddAttribute(23, "style", $"display: inline-block; width: {ItemSize}px; height: 200px; box-sizing: border-box;");
                // Render the provided item template.
                if (ItemTemplate is not null)
                {
                    builder.AddContent(24, ItemTemplate(item));
                }
                builder.CloseElement();
            }

            // Right spacer div: remaining width after the last rendered item.
            double rightSpacerWidth = Math.Max(0, Items.Count - _lastIndex) * ItemSize;
            builder.OpenElement(25, "div");
            builder.AddAttribute(26, "style", $"display: inline-block; width: {rightSpacerWidth}px;");
            builder.CloseElement();

            builder.CloseElement(); // End of outer container.
        }



        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var scrollInfo = await JSRuntime.InvokeAsync<ScrollInfo>("getScrollInfo", _containerId);
                maxScroll = scrollInfo.MaxScroll;
                scrollLeft = scrollInfo.ScrollLeft;
                Console.WriteLine(maxScroll);
                Console.WriteLine(scrollLeft);
                StateHasChanged();
            }
        }

        private async Task MoveLeftAsync()
        {
            // Get the current scroll position.
            scrollLeft = await JSRuntime.InvokeAsync<double>("eval", $"document.getElementById('{_containerId}').scrollLeft");

            // Calculate how many items can fully fit in the container.
            // This assumes ContainerWidth and ItemSize are numbers in the same unit (like pixels).
            int visibleItemCount = (int)(ContainerWidth / ItemSize);

            // Use the smaller of NavJumpItemCount or visibleItemCount as the jump count.
            int jumpCount = Math.Min(NavJumpItemCount, visibleItemCount);
            if (jumpCount < 1)
            {
                jumpCount = 1;
            }
            // Calculate the new scroll position (ensure it doesn’t go negative).
            double newScrollLeft = Math.Max(0, scrollLeft - (ItemSize * jumpCount));
            // Update the scroll position.
            await JSRuntime.InvokeVoidAsync("eval", $"document.getElementById('{_containerId}').scrollLeft = {newScrollLeft}");
            // Refresh virtualization.
            await OnScrollAsync();
        }

        /// <summary>
        /// Moves the container view one item to the right.
        /// </summary>
        private async Task MoveRightAsync()
        {
            // Get the current scroll position.
            scrollLeft = await JSRuntime.InvokeAsync<double>("eval", $"document.getElementById('{_containerId}').scrollLeft");

            // Calculate how many items can fully fit in the container.
            // This assumes ContainerWidth and ItemSize are numbers in the same unit (like pixels).
            int visibleItemCount = (int)(ContainerWidth / ItemSize);

            // Use the smaller of NavJumpItemCount or visibleItemCount as the jump count.
            int jumpCount = Math.Min(NavJumpItemCount, visibleItemCount);

            if (jumpCount < 1)
            {
                jumpCount = 1;
            }
            // Calculate the new scroll position by adding one item width.
            double newScrollLeft = scrollLeft + (ItemSize * jumpCount);
            // Update the scroll position.
            await JSRuntime.InvokeVoidAsync("eval", $"document.getElementById('{_containerId}').scrollLeft = {newScrollLeft}");
            // Refresh virtualization.
            await OnScrollAsync();
        }

        private async Task OnMouseDown(MouseEventArgs e)
        {
            _isDragging = true;
            _startX = e.ClientX;
            var scrollInfo = await JSRuntime.InvokeAsync<ScrollInfo>("getScrollInfo", _containerId);
            _scrollStartX = scrollInfo.ScrollLeft;

        }

        private async Task OnMouseMove(MouseEventArgs e)
        {
            if (!_isDragging)
                return;

            double deltaX = e.ClientX - _startX;
            double newScrollLeft = _scrollStartX - deltaX; // Flip direction for natural movement

            await JSRuntime.InvokeVoidAsync(
                "eval", $"document.getElementById('{_containerId}').scrollLeft = {newScrollLeft}"
            );
        }

        private void OnMouseUp(MouseEventArgs e)
        {
            if (!_isDragging)
                return;

            _isDragging = false;

        }



        public class ScrollInfo
        {
            public double ScrollLeft { get; set; }
            public double MaxScroll { get; set; }
        }
        #endregion
    }
}
