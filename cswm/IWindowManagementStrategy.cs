using cswm.Win32;

namespace cswm
{
    public interface IWindowManagementStrategy
    {
        IEnumerable<WindowLayout> Apply(IEnumerable<WindowDetails> windowDetails);
    }
}
