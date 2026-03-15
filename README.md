# Blazor Horizontal Virtualization (POC)

This project is a **proof of concept for horizontal virtualization in
Blazor**.

Blazor provides the `Virtualize` component for vertical lists, but it
does not support horizontal virtualization out of the box. This project
experiments with implementing a horizontal version inspired by Blazor's
original virtualization concept.

The component is implemented using **RenderTreeBuilder** and
**JavaScript interop** to track the scroll position and update the
visible item range dynamically.

## Features

-   Horizontal virtualization for large collections
-   Inspired by Blazor's built-in `Virtualize` component
-   Lightweight implementation using `BuildRenderTree`
-   Overscan rendering for smoother scrolling
-   Drag-to-scroll interaction
-   Optional navigation buttons

## How it Works

The component renders only the items that are visible in the viewport
plus a small buffer.

To simulate the full scrollable width:

-   A **left spacer** represents items before the visible range
-   A **right spacer** represents items after the visible range

When the user scrolls horizontally, the component calculates the new
visible range and re-renders only the necessary items.

## Parameters

  -----------------------------------------------------------------------
  Parameter                      Description
  ------------------------------ ----------------------------------------
  `Items`                        Collection of items to render

  `ItemTemplate`                 Template used to render each item

  `ContainerWidth`               Width of the scroll container

  `ItemSize`                     Width of each item

  `OverScanCount`                Number of extra items rendered outside
                                 the visible range

  `NavJumpItemCount`             Number of items to jump when using
                                 navigation buttons
  -----------------------------------------------------------------------

## Example

``` razor
<HorizontalVirtualization Items="items"
                          ItemSize="120"
                          ContainerWidth="800">
    <ItemTemplate Context="item">
        <div class="card">@item.Name</div>
    </ItemTemplate>
</HorizontalVirtualization>
```

## Notes

This is **only a proof of concept** and may require additional
improvements before production use.
