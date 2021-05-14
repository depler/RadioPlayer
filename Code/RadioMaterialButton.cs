using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Button;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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