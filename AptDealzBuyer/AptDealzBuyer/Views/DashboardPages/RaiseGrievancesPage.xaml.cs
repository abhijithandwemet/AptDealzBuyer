﻿using AptDealzBuyer.Model;
using AptDealzBuyer.Utility;
using AptDealzBuyer.Views.PopupPages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AptDealzBuyer.Views.DashboardPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RaiseGrievancesPage : ContentPage
    {
        #region Objects
        public List<RaiseGrievanceM> RaiseGrievanceMs = new List<RaiseGrievanceM>();
        private string filterBy = "";
        private string title = string.Empty;
        private bool? sortBy = null;
        private readonly int pageSize = 10;
        private int pageNo;
        #endregion

        #region Constructor
        public RaiseGrievancesPage()
        {
            InitializeComponent();
        }
        #endregion

        #region Methods
        protected override void OnAppearing()
        {
            base.OnAppearing();
            GetRaiseGrievance();
        }

        public void GetRaiseGrievance()
        {
            lstGrievance.ItemsSource = null;
            RaiseGrievanceMs = new List<RaiseGrievanceM>()
            {
                new RaiseGrievanceM{ GrNo="INV#123", GrPrice="Rs 2143", GrStatus="Shipped", GrDate="12-05-2021"},
                new RaiseGrievanceM{ GrNo="INV#456", GrPrice="Rs 2143", GrStatus="Accepted", GrDate="12-05-2021"},
                new RaiseGrievanceM{ GrNo="INV#789", GrPrice="Rs 2143", GrStatus="Completed", GrDate="12-05-2021"},
                new RaiseGrievanceM{ GrNo="INV#012", GrPrice="Rs 2143", GrStatus="Shipped", GrDate="12-05-2021"},
                new RaiseGrievanceM{ GrNo="INV#345", GrPrice="Rs 2143", GrStatus="Accepted", GrDate="12-05-2021"},
                new RaiseGrievanceM{ GrNo="INV#678", GrPrice="Rs 2143", GrStatus="Completed", GrDate="12-05-2021"},
                new RaiseGrievanceM{ GrNo="INV#901", GrPrice="Rs 2143", GrStatus="Shipped", GrDate="12-05-2021"},
                new RaiseGrievanceM{ GrNo="INV#234", GrPrice="Rs 2143", GrStatus="Accepted", GrDate="12-05-2021"},
            };
            lstGrievance.ItemsSource = RaiseGrievanceMs.ToList();
        }
        #endregion

        #region Events
        private void ImgMenu_Tapped(object sender, EventArgs e)
        {
            Common.BindAnimation(image: ImgMenu);
            //Common.OpenMenu();
        }

        private void ImgNotification_Tapped(object sender, EventArgs e)
        {

        }

        private void ImgQuestion_Tapped(object sender, EventArgs e)
        {

        }

        private void ImgBack_Tapped(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private void ImgSearch_Tapped(object sender, EventArgs e)
        {

        }

        private void FrmSortBy_Tapped(object sender, EventArgs e)
        {
            //var sortby = new PrevReqPopup("Sort By", "ID", "Quotes");
            //sortby.isRefresh += (s1, e1) =>
            //{
            //    //get result from popup
            //};
            //PopupNavigation.Instance.PushAsync(sortby);
            try
            {
                if (ImgSort.Source.ToString().Replace("File: ", "") == Constraints.Sort_ASC)
                {
                    ImgSort.Source = Constraints.Sort_DSC;
                    sortBy = false;
                }
                else
                {
                    ImgSort.Source = Constraints.Sort_ASC;
                    sortBy = true;
                }

                pageNo = 1;
                GetRaiseGrievance();
                //GetRaiseGrievance(filterBy, sortBy);
            }
            catch (Exception ex)
            {
                Common.DisplayErrorMessage("ActiveRequirementView/FrmSortBy_Tapped: " + ex.Message);
            }
        }

        private async void FrmStatus_Tapped(object sender, EventArgs e)
        {
            try
            {
                StatusPopup statusPopup = new StatusPopup(filterBy, "OrderSupplying");
                statusPopup.isRefresh += (s1, e1) =>
                {
                    string result = s1.ToString();
                    if (!Common.EmptyFiels(result))
                    {
                        //BindList
                    }
                };
                await PopupNavigation.Instance.PushAsync(statusPopup);
            }
            catch (Exception ex)
            {
                Common.DisplayErrorMessage("QuoteView/FrmStatusBy_Tapped: " + ex.Message);
            }
        }

        private void FrmSelect_Tapped(object sender, EventArgs e)
        {

        }

        private void lstGrievance_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            lstGrievance.SelectedItem = null;
        }

        private void FrmFilterBy_Tapped(object sender, EventArgs e)
        {
            try
            {
                var sortby = new FilterPopup(filterBy, "Active");
                sortby.isRefresh += (s1, e1) =>
                {
                    string result = s1.ToString();
                    if (!Common.EmptyFiels(result))
                    {
                        filterBy = result;
                        if (filterBy == RequirementSortBy.TotalPriceEstimation.ToString())
                        {
                            lblFilterBy.Text = "Amount";
                        }
                        else
                        {
                            lblFilterBy.Text = filterBy;
                        }
                        pageNo = 1;
                        GetRaiseGrievance();
                    }
                };
                PopupNavigation.Instance.PushAsync(sortby);
            }
            catch (Exception ex)
            {
                Common.DisplayErrorMessage("ActiveRequirementView/CustomEntry_Unfocused: " + ex.Message);
            }
        }
        #endregion

        private void BtnLogo_Clicked(object sender, EventArgs e)
        {
            Common.MasterData.Detail = new NavigationPage(new MainTabbedPages.MainTabbedPage("Home"));
        }
    }
}