using System;

namespace BluehatGames
{
    [Serializable]
    public class Wallet
    {
        public string address;
        public string privateKey;
        public string klaytnWalletKey;

        public Wallet(string address, string privateKey, string klaytnWalletKey)
        {
            this.address = address;
            this.privateKey = privateKey;
            this.klaytnWalletKey = klaytnWalletKey;
        }
    }
}