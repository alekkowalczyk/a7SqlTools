using a7SqlTools.Config.Model;
using a7SqlTools.Connection;
using a7SqlTools.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7SqlTools.Dialogs
{
    public class ConnectViewModel : ViewModelBase
    {
        public Dictionary<AuthType, string> AuthenticationTypes { get; set; }

        private AuthType _selectedAuthenticationType;

        public AuthType SelectedAuthenticationType
        {
            get { return _selectedAuthenticationType; }
            set { _selectedAuthenticationType = value; OnPropertyChanged(); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; OnPropertyChanged(); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged(); }
        }

        private string _errorMessage;

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public string ConnectionString => 
            ConnectionStringGenerator.Get(SelectedAuthenticationType, Name, UserName, Password);

        public ConnectViewModel()
        {
            this.Name = "localhost";
            AuthenticationTypes = new Dictionary<AuthType, string>
            {
                [AuthType.Windows] = "Windows Authentication",
                [AuthType.Sql] = "SQL Server Authentication"
            };
            SelectedAuthenticationType = AuthType.Windows;
        }

        public ConnectionData ToConnectionData() =>
            new ConnectionData
            {
                Name = this.Name,
                AuthType = this.SelectedAuthenticationType,
                UserName = this.UserName,
                Password = this.Password
            };
    }
}
