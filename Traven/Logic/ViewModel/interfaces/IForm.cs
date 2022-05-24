using System.Windows.Controls;
using Traven.Logic.ViewModel.classes;

namespace Traven.Logic.ViewModel.interfaces
{
    public interface IForm
    {
        Grid getStrokeFromNode(ANodeVM node);
    }
}
