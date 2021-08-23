﻿using Acr.UserDialogs;
using AptDealzBuyer.API;
using AptDealzBuyer.Interfaces;
using AptDealzBuyer.Model;
using AptDealzBuyer.Model.Reponse;
using AptDealzBuyer.Model.Request;
using AptDealzBuyer.Repository;
using AptDealzBuyer.Utility;
using Newtonsoft.Json.Linq;
using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AptDealzBuyer.Views.DashboardPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QuoteDetailsPage : ContentPage
    {
        #region [ Objects ]
        private QuoteAPI quoteAPI;
        private OrderAPI orderAPI;
        private Quote mQuote;

        private string QuoteId;
        #endregion

        #region [ Constructor ]
        public QuoteDetailsPage(string quoteId)
        {
            try
            {
                InitializeComponent();
                quoteAPI = new QuoteAPI();
                orderAPI = new OrderAPI();
                QuoteId = quoteId;
                mQuote = new Quote();

                MessagingCenter.Unsubscribe<string>(this, "NotificationCount"); MessagingCenter.Subscribe<string>(this, "NotificationCount", (count) =>
                {
                    if (!Common.EmptyFiels(Common.NotificationCount))
                    {
                        lblNotificationCount.Text = count;
                        frmNotification.IsVisible = true;
                    }
                    else
                    {
                        frmNotification.IsVisible = false;
                        lblNotificationCount.Text = string.Empty;
                    }
                });
            }
            catch (Exception ex)
            {
                Common.DisplayErrorMessage("QuoteDetailsPage/Ctor: " + ex.Message);
            }
        }
        #endregion

        #region [ Methods ]        
        public void Dispose()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Dispose();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            GetQuoteDetails();
        }

        private async Task GetQuoteDetails()
        {
            try
            {
                mQuote = await DependencyService.Get<IQuoteRepository>().GetQuoteById(QuoteId);
                if (mQuote != null)
                {
                    await GetRequirementsDetails();

                    lblRequirementId.Text = mQuote.RequirementNo;
                    lblQuoteRefNo.Text = mQuote.QuoteNo;
                    lblBuyerId.Text = mQuote.BuyerId;
                    lblSellerId.Text = mQuote.SellerId;
                    lblUnitPrice.Text = "Rs " + mQuote.UnitPrice;
                    lblCountryOrigin.Text = mQuote.Country;
                    lblTotalAmount.Text = "Rs " + mQuote.TotalQuoteAmount;


                    if (!Common.EmptyFiels(mQuote.ShippingPinCode))
                    {
                        lblShippingPINCode.Text = mQuote.ShippingPinCode;
                    }
                    if (!Common.EmptyFiels(mQuote.RequestedQuantity.ToString()))
                    {
                        lblRequestedQuantity.Text = mQuote.RequestedQuantity + " " + mQuote.Unit;
                    }
                    if (!Common.EmptyFiels(mQuote.NetAmount.ToString()))
                    {
                        lblNetAmount.Text = "Rs " + mQuote.NetAmount;
                    }
                    if (!Common.EmptyFiels(mQuote.HandlingCharges.ToString()))
                    {
                        lblHandlingCharges.Text = "Rs " + mQuote.HandlingCharges;
                    }
                    if (!Common.EmptyFiels(mQuote.ShippingCharges.ToString()))
                    {
                        lblShippingCharges.Text = "Rs " + mQuote.ShippingCharges;
                    }
                    if (!Common.EmptyFiels(mQuote.InsuranceCharges.ToString()))
                    {
                        lblInsuranceCharges.Text = "Rs " + mQuote.InsuranceCharges;
                    }
                    if (!Common.EmptyFiels(mQuote.Comments))
                    {
                        lblComments.Text = mQuote.Comments;
                    }

                    if (mQuote.IsSellerContactRevealed)
                    {
                        if (mQuote.SellerContact != null)
                            BtnRevealContact.Text = mQuote.SellerContact.PhoneNumber;
                    }
                    else
                    {
                        BtnRevealContact.Text = "Reveal Contact";
                    }

                    if (mQuote.Days.Contains("Expired"))
                    {

                        lblDate.Text = mQuote.ValidityDate.Date.ToString("dd/MM/yyyy") + " ( Expired )";
                    }
                    else
                    {
                        lblDate.Text = mQuote.ValidityDate.Date.ToString("dd/MM/yyyy");
                    }

                    //if (mQuote.Status == "Accepted")
                    if (mQuote.Status == "Accepted" || mQuote.Days.Contains("Expired"))
                    {
                        BtnAcceptQuote.IsEnabled = false;
                    }
                    else
                    {
                        BtnAcceptQuote.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.DisplayErrorMessage("QuoteDetailsPage/BindQuoteDetails: " + ex.Message);
            }
        }

        private async Task GetRequirementsDetails()
        {
            try
            {
                var mRequirement = await DependencyService.Get<IRequirementRepository>().GetRequirementById(mQuote.RequirementId);
                if (mRequirement != null)
                {
                    if (mRequirement.PickupProductDirectly)
                    {
                        lblShippingPinCode.Text = "Product Pickup PIN Code";
                    }
                    else
                    {
                        lblShippingPinCode.Text = "Shipping PIN Code";
                    }
                }

            }
            catch (Exception ex)
            {
                Common.DisplayErrorMessage("QuoteDetailsPage/GetRequirementsDetails: " + ex.Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        private async Task<Order> CreateOrder()
        {
            Order mOrder = new Order();
            try
            {
                CreateOrder mCreateOrder = new CreateOrder();
                mCreateOrder.QuoteId = mQuote.QuoteId;
                mCreateOrder.PaidAmount = mQuote.TotalQuoteAmount;

                UserDialogs.Instance.ShowLoading(Constraints.Loading);
                var mResponse = await orderAPI.CreateOrder(mCreateOrder);
                if (mResponse != null && mResponse.Succeeded)
                {
                    var jObject = (JObject)mResponse.Data;
                    if (jObject != null)
                    {
                        mOrder = jObject.ToObject<Order>();
                        mOrder.OrderSuccessMessage = mResponse.Message;
                        mOrder.IsSuccess = true;
                    }
                }
                else
                {
                    if (mResponse != null && !Common.EmptyFiels(mResponse.Message))
                        mOrder.OrderErrorMessage = mResponse.Message;
                    else
                        mOrder.OrderErrorMessage = Constraints.Something_Wrong;

                    mOrder.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                Common.DisplayErrorMessage("QuoteDetailsPage/CreateOrder: " + ex.Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
            return mOrder;
        }

        /// <summary>
        ///  [ Create Order | Payment Process | Update Order Payment Status | Accept Quote ]
        /// </summary>
        private async void PaidQuote()
        {
            try
            {
                var mOrder = await CreateOrder();
                if (mOrder != null && mOrder.IsSuccess)
                {
                    if (DeviceInfo.Platform == DevicePlatform.Android)
                    {
                        OpenRazorPayQuote(mOrder);
                    }
                    else
                    {
                        await AcceptQuote(mOrder.OrderSuccessMessage);
                        OnAppearing();
                    }
                }
                else
                {
                    if (mOrder.OrderErrorMessage.Contains("duplicate"))
                    {
                        OpenRazorPayQuote(mOrder);
                    }
                    else
                    {
                        Common.DisplayErrorMessage(mOrder.OrderErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.DisplayErrorMessage("QuoteDetailsPage/PaidQuote: " + ex.Message);
            }
        }

        /// <summary>
        /// [ Step 1 - CreateOrder ]
        /// </summary>
        private async Task QuotePayment(bool isRevealContact)
        {
            try
            {
                string message = string.Empty;
                if (isRevealContact)
                {
                    long revealRs = (long)App.Current.Resources["RevealContact"];
                    message = "You need to pay Rs " + revealRs + " to reveal the Seller contact information. Do you wish to continue making payment?";
                }
                else
                {
                    message = "Make a payment of Rs " + mQuote.TotalQuoteAmount + " to Accept Quote";
                }
                var contactPopup = new PopupPages.PaymentPopup(message);
                contactPopup.isRefresh += (s1, e1) =>
               {
                   bool isPay = (bool)s1;
                   if (isPay)
                   {
                       if (DeviceInfo.Platform == DevicePlatform.Android)
                       {
                           if (isRevealContact)
                           {
                               RevealSellerContact();
                           }
                           else
                           {
                               PaidQuote();
                           }
                       }
                       else
                       {
                           //PaidQuote(false);
                       }
                   }
               };
                await PopupNavigation.Instance.PushAsync(contactPopup);
            }
            catch (Exception ex)
            {
                Common.DisplayErrorMessage("QuoteDetailsPage/QuotePayment: " + ex.Message);
            }
        }

        public async Task OrderPayment(RazorResponse mRazorResponse)
        {
            try
            {
                OrderPayment mOrderPayment = new OrderPayment();
                mOrderPayment.OrderId = mRazorResponse.OrderNo;
                mOrderPayment.RazorPayPaymentId = mRazorResponse.PaymentId;
                mOrderPayment.RazorPayOrderId = mRazorResponse.OrderId;
                mOrderPayment.PaymentStatus = mRazorResponse.isPaid ? (int)Utility.PaymentStatus.Success : (int)Utility.PaymentStatus.Failed;

                UserDialogs.Instance.ShowLoading(Constraints.Loading);
                var mResponse = await orderAPI.CreateOrderPayment(mOrderPayment);
                if (mResponse != null && mResponse.Succeeded)
                {
                }
                else
                {
                    if (mResponse != null && !Common.EmptyFiels(mResponse.Message))
                        Common.DisplayErrorMessage(mResponse.Message);
                    else
                        Common.DisplayErrorMessage(Constraints.Something_Wrong);
                }
            }
            catch (Exception ex)
            {
                Common.DisplayErrorMessage("OrderRepository/OrderPayment: " + ex.Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// [ Step 2 - Payment Process with orderId ]
        /// </summary>
        /// <param name="mOrder"></param>
        private void OpenRazorPayQuote(Order mOrder)
        {
            try
            {
                RazorPayload payload = new RazorPayload();
                payload.amount = Convert.ToInt64(mQuote.TotalQuoteAmount * 100);
                payload.currency = (string)App.Current.Resources["Currency"];
                payload.receipt = mOrder.OrderNo; // Order NO

                payload.email = Common.mBuyerDetail.Email;
                payload.contact = Common.mBuyerDetail.PhoneNumber;

                MessagingCenter.Send<RazorPayload>(payload, "PayNow");

                MessagingCenter.Subscribe<RazorResponse>(this, "PaidResponse", async (razorResponse) =>
                {
                    // Update Payment status
                    razorResponse.OrderNo = mOrder.OrderId;
                    await OrderPayment(razorResponse);

                    if (razorResponse != null && razorResponse.isPaid)
                    {
                        await AcceptQuote(mOrder.OrderSuccessMessage);
                        OnAppearing();
                    }
                    else
                    {
                        string message = "Payment failed ";

                        if (Common.EmptyFiels(razorResponse.OrderId))
                            message += "OrderId: " + razorResponse.OrderId + " ";

                        if (Common.EmptyFiels(razorResponse.PaymentId))
                            message += "PaymentId: " + razorResponse.PaymentId + " ";

                        var contactPopup = new PopupPages.SuccessPopup(message, false);
                        await PopupNavigation.Instance.PushAsync(contactPopup);
                    }

                    MessagingCenter.Unsubscribe<RazorResponse>(this, "PaidResponse");
                });
            }
            catch (Exception ex)
            {
                Common.DisplayErrorMessage("QuoteDetailsPage/OpenPaymentPopup: " + ex.Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// [ Step 3 - Update Status of quote ]
        /// </summary>
        /// <returns></returns>
        private async Task AcceptQuote(string orderSuccessMessage)
        {
            try
            {
                UserDialogs.Instance.ShowLoading(Constraints.Loading);
                var mResponse = await quoteAPI.AcceptQuote(QuoteId);
                if (mResponse != null && mResponse.Succeeded)
                {
                    if (Common.EmptyFiels(orderSuccessMessage))
                    {
                        Common.DisplaySuccessMessage(orderSuccessMessage);
                    }
                }
                else
                {
                    if (mResponse != null)
                        Common.DisplayErrorMessage(mResponse.Message);
                    else
                        Common.DisplayErrorMessage(Constraints.Something_Wrong);
                }
            }
            catch (Exception ex)
            {
                Common.DisplayErrorMessage("QuoteDetailsPage/AcceptQuote: " + ex.Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        private void RevealSellerContact()
        {
            try
            {
                long amount = (long)App.Current.Resources["RevealContact"];
                RazorPayload payload = new RazorPayload();
                payload.amount = amount * 100;
                payload.currency = (string)App.Current.Resources["Currency"];
                payload.receipt = QuoteId; // quoteid
                payload.email = Common.mBuyerDetail.Email;
                payload.contact = Common.mBuyerDetail.PhoneNumber;
                MessagingCenter.Send<RazorPayload>(payload, "RevealPayNow");
                MessagingCenter.Subscribe<RazorResponse>(this, "PaidRevealResponse", async (razorResponse) =>
                {
                    if (razorResponse != null && !razorResponse.isPaid)
                    {
                        string message = "Payment failed ";

                        if (!Common.EmptyFiels(razorResponse.OrderId))
                            message += "OrderId: " + razorResponse.OrderId + " ";

                        if (!Common.EmptyFiels(razorResponse.PaymentId))
                            message += "PaymentId: " + razorResponse.PaymentId + " ";

                        if (message != null)
                            Common.DisplayErrorMessage(message);
                    }

                    RevealSellerContact mRevealSellerContact = new RevealSellerContact();
                    mRevealSellerContact.QuoteId = QuoteId;
                    mRevealSellerContact.PaymentStatus = razorResponse.isPaid ? (int)RevealContactStatus.Success : (int)RevealContactStatus.Failure;
                    mRevealSellerContact.RazorPayOrderId = razorResponse.OrderId;
                    mRevealSellerContact.RazorPayPaymentId = razorResponse.PaymentId;

                    BtnRevealContact.Text = await DependencyService.Get<IQuoteRepository>().RevealContact(mRevealSellerContact);

                    MessagingCenter.Unsubscribe<RazorResponse>(this, "PaidRevealResponse");
                });

            }
            catch (Exception ex)
            {
                Common.DisplayErrorMessage("QuoteDetailsPage/OpenPaymentPopup: " + ex.Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        private async void RejectQuote()
        {
            try
            {
                UserDialogs.Instance.ShowLoading(Constraints.Loading);
                var mResponse = await quoteAPI.RejectQuote(QuoteId);
                if (mResponse != null && mResponse.Succeeded)
                {
                    Common.DisplaySuccessMessage(mResponse.Message);
                }
                else
                {
                    if (mResponse != null)
                        Common.DisplayErrorMessage(mResponse.Message);
                    else
                        Common.DisplayErrorMessage(Constraints.Something_Wrong);
                }
            }
            catch (Exception ex)
            {
                Common.DisplayErrorMessage("QuoteDetailsPage/RejectQuote: " + ex.Message);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }
        #endregion

        #region [ Events ]        
        private void ImgMenu_Tapped(object sender, EventArgs e)
        {
            Common.BindAnimation(image: ImgMenu);
            //Common.OpenMenu();
        }

        private async void ImgNotification_Tapped(object sender, EventArgs e)
        {
            var Tab = (Grid)sender;
            if (Tab.IsEnabled)
            {
                try
                {
                    Tab.IsEnabled = false;
                    await Navigation.PushAsync(new NotificationPage());
                }
                catch (Exception ex)
                {
                    Common.DisplayErrorMessage("QuoteDetailsPage/ImgNotification_Tapped: " + ex.Message);
                }
                finally
                {
                    Tab.IsEnabled = true;
                }
            }
        }

        private void ImgQuestion_Tapped(object sender, EventArgs e)
        {

        }

        private async void ImgBack_Tapped(object sender, EventArgs e)
        {
            Common.BindAnimation(imageButton: ImgBack);
            await Navigation.PopAsync();
        }

        private async void BtnRevealContact_Clicked(object sender, EventArgs e)
        {
            var Tab = (Button)sender;
            if (Tab.IsEnabled)
            {
                try
                {
                    Tab.IsEnabled = false;
                    Common.BindAnimation(button: BtnRevealContact);
                    if (BtnRevealContact.Text == "Reveal Contact")
                    {
                        await QuotePayment(true);
                    }
                    else
                    {
                        DependencyService.Get<IDialer>().Dial(App.Current.Resources["CountryCode"] + BtnRevealContact.Text);
                    }
                }
                catch (Exception ex)
                {
                    Common.DisplayErrorMessage("QuoteDetailsPage/BtnRevealContact_Clicked: " + ex.Message);
                }
                finally
                {
                    Tab.IsEnabled = true;
                }
            }
        }

        private async void BtnAcceptQuote_Clicked(object sender, EventArgs e)
        {
            var Tab = (Button)sender;
            if (Tab.IsEnabled)
            {
                try
                {
                    Tab.IsEnabled = false;
                    Common.BindAnimation(button: BtnAcceptQuote);
                    await QuotePayment(false);
                }
                catch (Exception ex)
                {
                    Common.DisplayErrorMessage("QuoteDetailsPage/BtnAcceptQuote_Clicked: " + ex.Message);
                }
                finally
                {
                    Tab.IsEnabled = true;
                }
            }

        }

        private async void BtnBackToQuote_Clicked(object sender, EventArgs e)
        {
            var Tab = (Button)sender;
            if (Tab.IsEnabled)
            {
                try
                {
                    Tab.IsEnabled = false;
                    Common.BindAnimation(button: BtnBackToQuote);
                    await Navigation.PopAsync();
                }
                catch (Exception ex)
                {
                    Common.DisplayErrorMessage("QuoteDetailsPage/BtnBackToQuote_Clicked: " + ex.Message);
                }
                finally
                {
                    Tab.IsEnabled = true;
                }
            }
        }

        private void BtnLogo_Clicked(object sender, EventArgs e)
        {
            Common.MasterData.Detail = new NavigationPage(new MainTabbedPages.MainTabbedPage("Home"));
        }

        private async void RefreshView_Refreshing(object sender, EventArgs e)
        {
            try
            {
                rfView.IsRefreshing = true;
                await GetQuoteDetails();
                rfView.IsRefreshing = false;
            }
            catch (Exception ex)
            {
                Common.DisplayErrorMessage("QuoteDetailsPage/RefreshView_Refreshing: " + ex.Message);
            }
        }

        private void CopyString_Tapped(object sender, EventArgs e)
        {
            try
            {
                var stackLayout = (StackLayout)sender;
                if (!Common.EmptyFiels(stackLayout.ClassId))
                {
                    if (stackLayout.ClassId == "RequirementId")
                    {
                        string message = Constraints.CopiedRequirementId;
                        Common.CopyText(lblRequirementId, message);
                    }
                    else if (stackLayout.ClassId == "QuoteRefNo")
                    {
                        string message = Constraints.CopiedQuoteRefNo;
                        Common.CopyText(lblQuoteRefNo, message);
                    }
                    else if (stackLayout.ClassId == "BuyerId")
                    {
                        string message = Constraints.CopiedBuyerId;
                        Common.CopyText(lblBuyerId, message);
                    }
                    else if (stackLayout.ClassId == "SellerId")
                    {
                        string message = Constraints.CopiedSellerId;
                        Common.CopyText(lblSellerId, message);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.DisplayErrorMessage("QuoteDetailsPage/CopyString_Tapped: " + ex.Message);
            }
        }

        //private async void BtnRejectQuote_Clicked(object sender, EventArgs e)
        //{
        //    Common.BindAnimation(button: BtnRejectQuote);
        //    RejectQuote();
        //    await Navigation.PopAsync();
        //}
        #endregion
    }
}