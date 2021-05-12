using System;

namespace Entity
{
    public record User
    {
        public Guid ID { get; set; }

        public string Name { get; set; }

        public string Account { get; set; }

        public string Password { get; set; }

        public string Sex { get; set; }

        public string Email { get; set; }

        public string Mobile { get; set; }
    }
}
