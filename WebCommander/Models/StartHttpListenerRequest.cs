namespace WebCommander.Models
{
    using System.ComponentModel.DataAnnotations;

    public class StartHttpListenerRequest
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Ip { get; set; }
        
        [Required]
        public int BindPort { get; set; }
        public bool Secured { get; set; }
    }
}
