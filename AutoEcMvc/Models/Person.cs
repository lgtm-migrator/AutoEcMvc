using System.ComponentModel.DataAnnotations;

namespace AutoEcMvc.Models
{
    public abstract partial class Person
    {
        [Display(Name = "Full Name")]
        public string FullName => LastName + ", " + FirstMidName;
    }
}
