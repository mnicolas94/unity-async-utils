using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AsyncUtils.Popup
{
    public class CloseButtonsPopup : AsyncPopup
    {
        [SerializeField] private List<Button> _closeButtons;

        public override void Initialize()
        {
        }

        public override async Task Show(CancellationToken ct)
        {
            await Utils.WaitFirstButtonPressedAsync(ct, _closeButtons.ToArray());
        }
    }
}