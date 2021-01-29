using FluentMigrator;
using Nop.Data.Migrations;

namespace NSS.Plugin.Misc.SwiftApi.Data
{
    [NopMigration("2021/01/29 07:15:00", "Swift.Api Update Values Add Period in LocaleStringResource")]
    public class UpdateLocaleStringResourceValues : Migration
    {
        public override void Up()
        {
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Enter discount code.' Where [ResourceName] = 'ShoppingCart.DiscountCouponCode.Label'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Check the heart icon next to a product in the catalog to add a favorite.' Where [ResourceName] = 'Wishlist.CartIsEmpty'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Login was unsuccessful.' Where [ResourceName] = 'Account.Login.Unsuccessful'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'If this is the email address of a valid user, we sent a link to reset the password.' Where [ResourceName] = 'Account.PasswordRecovery.EmailNotFound'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Product is no longer available.' Where [ResourceName] = 'ShoppingCart.ProductUnpublished'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Change password.' Where [ResourceName] = 'Account.ChangePassword'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Email is not entered.' Where [ResourceName] = 'Account.ChangePassword.Errors.EmailIsNotProvided'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'The specified email could not be found.' Where [ResourceName] = 'Account.ChangePassword.Errors.EmailNotFound'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Old password doesn\'t match.' Where [ResourceName] = 'Account.ChangePassword.Errors.OldPasswordDoesntMatch'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Password is not entered.' Where [ResourceName] = 'Account.ChangePassword.Errors.PasswordIsNotProvided'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'You entered the password that is the same as one of the last passwords you used. Please create a new password.' Where [ResourceName] = 'Account.ChangePassword.Errors.PasswordMatchesWithPrevious'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Password is required.' Where [ResourceName] = 'Account.ChangePassword.Fields.ConfirmNewPassword.Required'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'The new password and confirmation password do not match.' Where [ResourceName] = 'Account.ChangePassword.Fields.NewPassword.EnteredPasswordsDoNotMatch'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Old password is required.' Where [ResourceName] = 'Account.ChangePassword.Fields.OldPassword.Required'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Password was changed.' Where [ResourceName] = 'Account.ChangePassword.Success'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'City is required.' Where [ResourceName] = 'Account.Fields.City.Required'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Company name is required.' Where [ResourceName] = 'Account.Fields.Company.Required'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'The new password and confirmation password do not match.' Where [ResourceName] = 'Account.PasswordRecovery.NewPassword.EnteredPasswordsDoNotMatch'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Your password has been changed.' Where [ResourceName] = 'Account.PasswordRecovery.PasswordHasBeenChanged'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Out of stock.' Where [ResourceName] = 'Products.Availability.OutOfStock'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'The minimum quantity allowed for purchase is {0}.' Where [ResourceName] = 'ShoppingCart.MinimumQuantity'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'The maximum number of distinct products allowed in the wishlist is {0}.' Where [ResourceName] = 'ShoppingCart.MaximumWishlistItems'");
        }                                                                                                                                         


        public override void Down()
        {
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Enter discount coupon code' Where [ResourceName] = 'ShoppingCart.DiscountCouponCode.Label'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'The wishlist is empty!' Where [ResourceName] = 'Wishlist.CartIsEmpty'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Login was unsuccessful. Please correct the errors and try again' Where [ResourceName] = 'Account.Login.Unsuccessful'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Email not found' Where [ResourceName] = 'Account.PasswordRecovery.EmailNotFound'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Product is no longer available' Where [ResourceName] = 'ShoppingCart.ProductUnpublished'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Change password' Where [ResourceName] = 'Account.ChangePassword'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Email is not entered' Where [ResourceName] = 'Account.ChangePassword.Errors.EmailIsNotProvided'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'The specified email could not be found' Where [ResourceName] = 'Account.ChangePassword.Errors.EmailNotFound'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Old password doesn\'t match' Where [ResourceName] = 'Account.ChangePassword.Errors.OldPasswordDoesntMatch'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Password is not entered' Where [ResourceName] = 'Account.ChangePassword.Errors.PasswordIsNotProvided'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'You entered the password that is the same as one of the last passwords you used. Please create a new password' Where [ResourceName] = 'Account.ChangePassword.Errors.PasswordMatchesWithPrevious'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Password is required' Where [ResourceName] = 'Account.ChangePassword.Fields.ConfirmNewPassword.Required'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'The new password and confirmation password do not match' Where [ResourceName] = 'Account.ChangePassword.Fields.NewPassword.EnteredPasswordsDoNotMatch'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Old password is required' Where [ResourceName] = 'Account.ChangePassword.Fields.OldPassword.Required'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Password was changed' Where [ResourceName] = 'Account.ChangePassword.Success'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'City is required' Where [ResourceName] = 'Account.Fields.City.Required'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Company name is required' Where [ResourceName] = 'Account.Fields.Company.Required'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'The new password and confirmation password do not match' Where [ResourceName] = 'Account.PasswordRecovery.NewPassword.EnteredPasswordsDoNotMatch'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Your password has been changed' Where [ResourceName] = 'Account.PasswordRecovery.PasswordHasBeenChanged'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Out of stock' Where [ResourceName] = 'Products.Availability.OutOfStock'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'The minimum quantity allowed for purchase is {0}' Where [ResourceName] = 'ShoppingCart.MinimumQuantity'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'The maximum number of distinct products allowed in the wishlist is {0}' Where [ResourceName] = 'ShoppingCart.MaximumWishlistItems'");
        }
    }
}
