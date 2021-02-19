using Akka.Actor;
using Neo.Network.P2P;
using Neo.Network.P2P.Payloads;
using Neo.Properties;
using Neo.SmartContract;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static Neo.Program;

namespace Neo.GUI
{
    internal static class Helper
    {
        private static readonly Dictionary<Type, Form> tool_forms = new Dictionary<Type, Form>();

        private static void Helper_FormClosing(object sender, FormClosingEventArgs e)
        {
            tool_forms.Remove(sender.GetType());
        }

        public static void Show<T>() where T : Form, new()
        {
            Type t = typeof(T);
            if (!tool_forms.ContainsKey(t))
            {
                tool_forms.Add(t, new T());
                tool_forms[t].FormClosing += Helper_FormClosing;
            }
            tool_forms[t].Show();
            tool_forms[t].Activate();
        }

        public static void SignAndShowInformation(Transaction tx)
        {
            if (tx == null)
            {
                MessageBox.Show(Strings.InsufficientFunds);
                return;
            }
            ContractParametersContext context;
            try
            {
                context = new ContractParametersContext(Service.NeoSystem.StoreView,tx);
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show(Strings.UnsynchronizedBlock);
                return;
            }
            Service.CurrentWallet.Sign(context);
            if (context.Completed)
            {
                tx.Witnesses = context.GetWitnesses();
                Service.NeoSystem.Blockchain.Tell(tx);
                InformationBox.Show(tx.Hash.ToString(), Strings.SendTxSucceedMessage, Strings.SendTxSucceedTitle);
            }
            else
            {
                InformationBox.Show(context.ToString(), Strings.IncompletedSignatureMessage, Strings.IncompletedSignatureTitle);
            }
        }
    }
}
