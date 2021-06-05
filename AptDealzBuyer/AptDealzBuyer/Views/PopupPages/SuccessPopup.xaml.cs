﻿using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms.Xaml;

namespace AptDealzBuyer.Views.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SuccessPopup : PopupPage
    {
        #region Objects
        // create objects here
        public event EventHandler isRefresh;
        #endregion

        #region Constructor
        public SuccessPopup()
        {
            InitializeComponent();
        }
        #endregion

        #region Methods
        // write methods here
        #endregion

        #region Events
        // create events here
        private void FrmHome_Tapped(object sender, EventArgs e)
        {
            isRefresh?.Invoke(true, EventArgs.Empty);
            PopupNavigation.Instance.PopAsync();
        }
        #endregion
    }
}