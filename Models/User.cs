using System.ComponentModel.DataAnnotations;

namespace UserManagementApi.Models {

    public class User {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        required public string Name { get; set; }
    }
}