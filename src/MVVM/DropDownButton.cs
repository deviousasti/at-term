using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace System.Windows.Controls
{
    public class DropDownButton : ToggleButton
    {
        // *** Dependency Properties ***

        public static readonly DependencyProperty DropDownProperty =
          DependencyProperty.Register("DropDown",
                                      typeof(ContextMenu),
                                      typeof(DropDownButton),
                                      new UIPropertyMetadata(null));

        // *** Constructors *** 

        public DropDownButton()
        {
            // Bind the ToogleButton.IsChecked property to the drop-down's IsOpen property 

            this.SetBinding(IsCheckedProperty, new Binding("DropDown.IsOpen")
            {
                Source = this
            });
        }

        // *** Properties *** 

        public ContextMenu DropDown
        {
            get { return (ContextMenu)this.GetValue(DropDownProperty); }
            set { this.SetValue(DropDownProperty, value); }
        }

        // *** Overridden Methods *** 

        protected override void OnClick()
        {
            if (this.DropDown != null)
            {
                // If there is a drop-down assigned to this button, then position and display it 

                this.DropDown.PlacementTarget = this;
                this.DropDown.Placement = PlacementMode.Center;

                this.DropDown.IsOpen = true;
            }
        }
    }
}