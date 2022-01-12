using Android.Content;
using Android.Util;
using Google.Android.Material.Button;
using System;
using System.Threading.Tasks;

namespace RadioPlayer.Code
{
    public class RadioMaterialButton : MaterialButton
    {
        public RadioMaterialButton(Context context) : base(context) { }

        public RadioMaterialButton(Context context, IAttributeSet attrs) : base(context, attrs) { }

        public RadioMaterialButton(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) { }

        public Func<Task<string>> GetRadioUrlAsync = null;
    }
}