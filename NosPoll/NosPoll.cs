using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.ComponentModel;
using System.Numerics;

namespace Neo.SmartContract
{
    public class NosPoll : Framework.SmartContract
    {
        public static byte[] OwnerAddr = "ATrzHaicmhRj15C3Vv6e6gLfLqhSD2PtTr".ToScriptHash();
        
        public static object Main(string operation, params object[] args)
        {
            Runtime.Notify("Main() operation", operation);

            bool VoteEnabled;
            bool VoteLimit;
            string name;
            string limitS;
            string VotingFlag;
            string voting = "voting";
            byte[] CallerAddr;
            
            // if the smart contract is invoked from Web app
            if (Runtime.Trigger == TriggerType.Application)
            {

                switch (operation)
                {
                    // When startvoting event is pushed from Web
                    case "StartVote":
                        VotingFlag = "1";
                        limitS = "1";
                        VoteEnabled = IsVotingEnabled(voting);
                        if (!VoteEnabled)
                        {
                            try
                            {
                                CallerAddr = (byte[]) args[0];
                            }
                            catch(FormatException e)
                            {
                                return e;
                            }
                            StartVote(VotingFlag,voting,limitS, CallerAddr);
                        }
                        break;

                    // When End voting event is pushed from Web					
                    case "EndVote":
                        VotingFlag = "0";
                        VoteEnabled = IsVotingEnabled(voting);
                        if (VoteEnabled)
                        {
                            try
                            {
                                CallerAddr = (byte[])args[0];
                            }
                            catch (FormatException e)
                            {
                                return e;
                            }
                            EndVote(VotingFlag, voting, CallerAddr);
                        }
                        break;

                    // When Cast voting event is pushed from Web						
                    case "CastVote":
                        limitS = "1";
                        try
                        {
                            CallerAddr   = (byte[]) args[0];
                            name         = (string) args[1];
                        }
                        catch(FormatException e)
                        {
                            return e;
                        }
                        VoteEnabled = IsVotingEnabled(voting);
                        VoteLimit = CheckVoteLimit(CallerAddr, name, voting);
                        if (VoteEnabled && VoteLimit)
                        {
                            CastVote(CallerAddr, name);
                        }
                        break;

                    // When voting limit needs to be set by user
                    case "SetLimit":
                        VoteEnabled = IsVotingEnabled(voting);
                        try
                        {
                            CallerAddr = (byte[]) args[0];
                            limitS     = (string) args[2];
                        }
                        catch(FormatException e)
                        {
                            return e;
                        }
                        if (VoteEnabled)
                        {
                            SetLimit(voting, limitS, CallerAddr);
                        }
                        break;

                    // Fetching the results of the vote
                    case "GetResults":
                        try
                        {
                            CallerAddr = (byte[]) args[0];
                            name       = (string) args[1];
                        }
                        catch (FormatException e)
                        {
                          return e;
                        }
                        VoteEnabled = IsVotingEnabled(voting);

                        if (!VoteEnabled)
                        {
                            return GetResults(name);
                        }
                        break;

                    default:
                        break;

                }
            }
            return "Success";
        }

        // Start the voting process
        public static void StartVote(string VotingFlag, string voting, string limitS, byte[] CallerAddr)
        {
            bool Authority = CheckAuth(CallerAddr);
            if (Authority)
            {
                Storage.Put(Storage.CurrentContext, voting, VotingFlag.Serialize());
                SetLimit(voting, limitS, CallerAddr);
            }
        }

        // End the voting process
        public static void EndVote(string VotingFlag, string voting, byte[] CallerAddr)
        {
            bool Authority = CheckAuth(CallerAddr);
            if (Authority)
            {
                Storage.Put(Storage.CurrentContext, voting, VotingFlag.Serialize());
            }
        }

        // Record the votes casted by each user
        public static void CastVote(byte[] CallerAddr, string name)
        {
            // Get the number of votes for the category
            uint numberofvotes   = (uint) Storage.Get(Storage.CurrentContext, name).Deserialize();
            numberofvotes++;

            // Get the number of votes done by individual
            uint IndividualVote  = (uint) Storage.Get(Storage.CurrentContext, CallerAddr).Deserialize();
            IndividualVote++; 

            // Save the value in persistant storage 
            Storage.Put(Storage.CurrentContext, name, numberofvotes.Serialize());
            Storage.Put(Storage.CurrentContext, CallerAddr, IndividualVote.Serialize());
        }

        // Set the number of voting limit for each person
        public static void SetLimit(string voting, string VotingLimit, byte[] CallerAddr)
        {
            bool Authority = CheckAuth(CallerAddr);
            if (Authority)
            {
                Storage.Put(Storage.CurrentContext, voting, VotingLimit.Serialize());
            }
        }

        // Check if the voting has started or not
        public static bool IsVotingEnabled(string voting)
        {
            uint VotingStarted = (uint) Storage.Get(Storage.CurrentContext, voting).Deserialize();
            if (VotingStarted == 1) 
              return true;
            else
              return false;
        }

        // Check if the user has crossed his voting limit 
        public static bool CheckVoteLimit(byte[] CallerAddr, string name, string voting)
        {
            uint VoteCap       = (uint) Storage.Get(Storage.CurrentContext, voting).Deserialize();
            uint numberofvotes = (uint) Storage.Get(Storage.CurrentContext, CallerAddr).Deserialize();
            if (numberofvotes >= VoteCap)
                return false;
            else
                return true;
        }

        // return the results of the poll
        public static string GetResults(string name)
        {
            return (string) Storage.Get(Storage.CurrentContext, name).Deserialize();
        }

        // Check if the person is authorized to change the limits
        public static bool CheckAuth(byte[] CallerAddr)
        {
            if (Runtime.CheckWitness(OwnerAddr) == Runtime.CheckWitness(CallerAddr))
               return true;
            else
               return false;
        }
    }

}