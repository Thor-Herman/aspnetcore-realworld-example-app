using System;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Check(string password, string hash);
}
