using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Horde.Core.Domains.World.Entities
{

    public class Registration : BaseEntity
    {
        [NotMapped]
        public override ContextNames Context => ContextNames.World;

        public int UserId { get; set; }
        public RegistrationStepType Step { get; set; }
        public string Name { get; set; }
        public string LastMessageReferenceId { get; set; }
        public string ProfilePic { get; set; }

        [Phone]
        public string PhonuNumber { get; set; }
        public string PaymentOption { get; set; }
        public string PaymentId { get; set; }
        public string Key1 { get; set; }
        public string Value1 { get; set; }
        public int RefferalUserId { get; set; }
        public static readonly string[] PaymentOptions = new string[] { "Paytm (Preferred)", "Upi" };
        [NotMapped]
        public User User { get; set; }
        [NotMapped]
        public string StepResult { get; internal set; }
        public bool MarkedAsFraud { get; set; }
    }
}

namespace Horde.Core
{
    public enum RegistrationStepType
    {
        Started, ProfilePicRequested, VerifyIgnRequested, VerifyProfileInfoRequested, PaymentOptionRequested, PaymentIdRequested, VerifyPaymentOptionsRequested,
        PhoneNumberRequested, VerifyPhoneNumberRequested, EmailIdRequested, VerifyEmaildRequested,
        VerificationComplete,

    }

}