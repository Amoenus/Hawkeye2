using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Hawkeye.Reflection;

namespace Hawkeye.UI.Controls
{
    // Search Box implementation
    internal partial class PropertyGridEx
    {
        private FieldAccessor _allGridEntriesAccessor;
        private GridItem[] _currentGridItems;
        private FieldAccessor _gridViewEntriesAccessor;

        private object _reflectedGridView;
        private MethodAccessor _refreshAccessor;
        private TextBox _searchBox;
        private Color _searchBoxBackColor = Color.White;

        private PropertyAccessor _selectedGridEntryAccessor;
        private FieldAccessor _selectedRowAccessor;

        private MethodAccessor _setScrollOffsetAccessor;
        private FieldAccessor _topLevelGridEntriesAccessor;
        private FieldAccessor _totalPropsAccessor;

        private void InitializeSearchBox()
        {
            if (_searchBox != null)
            {
                return;
            }

            _searchBox = new TextBox();
            _searchBox.Location = new Point(0, 0);
            _searchBox.Size = new Size(70, _searchBox.Height);
            _searchBox.BorderStyle = BorderStyle.Fixed3D;
            _searchBox.Font = new Font("Tahoma", 8.25f);

            _searchBoxBackColor = _searchBox.BackColor;

            _searchBox.TextChanged += (s, _) => ApplyFilter();

            // Hack: let's remove the read-only flag on the toolstrip controls collection
            var rofield = new FieldAccessor(ToolStrip.Controls, "_isReadOnly");
            rofield.Set(false);
            ToolStrip.Controls.Add(_searchBox);
            rofield.Set(true);

            ToolStrip.SizeChanged += (s, _) => FixSearchBoxLocation();

            FixSearchBoxLocation();

            // And now initialize accessors
            InitializeAccessors();

            PropertyTabChanged += (s, _) => _searchBox.Text = string.Empty;
            PropertySortChanged += (s, _) => _searchBox.Text = string.Empty;
            SelectedObjectsChanged += (s, _) => _searchBox.Text = string.Empty;
        }

        private void InitializeAccessors()
        {
            var gridViewAccessor = new FieldAccessor(this, "gridView");
            _reflectedGridView = gridViewAccessor.Get();
            Type gridViewType = _reflectedGridView.GetType();

            _allGridEntriesAccessor = new FieldAccessor(_reflectedGridView, "allGridEntries");
            _topLevelGridEntriesAccessor = new FieldAccessor(_reflectedGridView, "topLevelGridEntries");
            _totalPropsAccessor = new FieldAccessor(_reflectedGridView, "totalProps");
            _selectedRowAccessor = new FieldAccessor(_reflectedGridView, "selectedRow");

            _setScrollOffsetAccessor = new MethodAccessor(gridViewType, "SetScrollOffset");
            _refreshAccessor = new MethodAccessor(gridViewType, "Refresh");

            _selectedGridEntryAccessor = new PropertyAccessor(_reflectedGridView, "SelectedGridEntry");
        }

        private void ApplyFilter()
        {
            string search = _searchBox.Text.ToLowerInvariant();

            if (string.IsNullOrEmpty(search))
            {
                _searchBox.BackColor = _searchBoxBackColor;
            }
            else
            {
                _searchBox.BackColor = search.StartsWith("?") ? Color.LightBlue : Color.Coral;
            }

            if (SelectedObject == null)
            {
                return;
            }

            GridItem[] items = GetGridViewItems();
            if (items == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(search) && _currentGridItems != null)
            {
                _currentGridItems = null;
                Refresh();
                return;
            }

            if (_currentGridItems == null)
            {
                _currentGridItems = items;
            }

            bool containsMode = search.StartsWith("?");
            if (containsMode)
            {
                search = search.Substring(1);
            }

            // Filter out
            GridItem[] keptItems = _currentGridItems.Where(item =>
            {
                if (string.IsNullOrEmpty(item.Label))
                {
                    return false;
                }

                if (item.GridItemType != GridItemType.Property)
                {
                    return false;
                }

                string label = item.Label.ToLowerInvariant();

                return containsMode ? label.Contains(search) : label.StartsWith(search);
            }).ToArray();

            SetGridViewItems(keptItems);
        }

        private GridItem[] GetGridViewItems()
        {
            object items = _allGridEntriesAccessor.Get();
            if (items == null)
            {
                return null;
            }

            if (_gridViewEntriesAccessor == null)
            {
                _gridViewEntriesAccessor = new FieldAccessor(items, "entries");
            }

            return _gridViewEntriesAccessor.Get(items) as GridItem[];
        }

        private void SetGridViewItems(GridItem[] newItems)
        {
            object items = _allGridEntriesAccessor.Get();
            if (items == null)
            {
                return;
            }

            if (newItems == null)
            {
                newItems = new GridItem[0];
            }

            bool wasFocused = _searchBox.Focused;

            if (_gridViewEntriesAccessor == null)
            {
                _gridViewEntriesAccessor = new FieldAccessor(items, "entries");
            }

            _setScrollOffsetAccessor.Invoke(_reflectedGridView, 0);

            _gridViewEntriesAccessor.Set(newItems, items);
            _gridViewEntriesAccessor.Set(newItems, _topLevelGridEntriesAccessor.Get());

            _totalPropsAccessor.Set(newItems.Length);
            _selectedRowAccessor.Set(0);

            if (newItems.Length > 0)
            {
                _selectedGridEntryAccessor.Set(newItems[0]);
            }

            ((Control) _reflectedGridView).Invalidate();

            if (wasFocused)
            {
                _searchBox.Focus();
            }
        }

        private void FixSearchBoxLocation()
        {
            _searchBox.Location = new Point(
                ToolStrip.Width - _searchBox.Width - 2,
                (ToolStrip.Height - _searchBox.Height) / 2);
        }
    }
}