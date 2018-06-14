using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Text;
using System.ComponentModel;
using System.Numerics;

namespace Neo.SmartContract
{
    public class NosPoll : Framework.SmartContract
    {
               
        public static object Main(string operation, params object[] args)
        {
            //  PARAMS detail
            //args[0] = Owner Address
            //args[1] = PollID
            //args[2] = Option Voted
            //args[3] = Question

            // TODO: CHECK HOW TO CREATE A UNIQUE ID NUMBER
            // IMPLEMENT ON CLICK EVENT 
            //		Random r = new Random();
			//		int rInt = r.Next(0, 1000);
			// This will give us an unique number on SC

            Runtime.Notify("Main() operation", operation);

            string OwnerAdd = args[0]; 
            string PollId = args[1];
            string Question = args[2];
            string VoteOption = args[3];
            string[] strArray = new string[4];
            string[] SCReturn = new string[3]
            int OptionA;
            int OptionB;
            int[] Votes = new int [1];
            string Question;

            
            // if the smart contract is invoked from Web app
            if (Runtime.Trigger == TriggerType.Application)
            {

                switch (operation)
                {
                    case "CreatePoll":
                    	strArray[0] = OwnerAdd;
                        strArray[1] = CreatePoll(string OwnerAdd, string Question);
                        strArray[2] = Question;
                        strArray[3] = "0";
                        strArray[4] = "0";
                        return strArray; 
                    case "AccessPoll":
                        strArray=AccessPoll(string OwnerAdd, string PollID);
                        SCReturn = PollInfo.Split("/");
            			//SCReturn[0]=OwnerAdd
            			//SCReturn[1]=PollID
            			//SCReturn[2]=Question
                        strArray[0] = SCReturn[0];
                        strArray[1] = SCReturn[1];
                        strArray[2] = SCReturn[2];
                        strArray[3] = "0";
                        strArray[4] = "0";
                        return strArray;
                    case "VotePoll":
                        VotePoll(string OwnerAdd, string PollID, string VoteOption);
                        break;
                    case "ResultsPoll":
                        Votes = ResultsPoll(string OwnerAdd, string PollID);
                        strArray[0] = OwnerAdd;
                        strArray[1] = PollID;
                        strArray[2] = Question;
                        Votes[0]=strArray[3].lenght - 1;
                        Votes[1]=strArray[4].lenght - 1;
                        return Votes;
                    default:
                        break;
                }
            }
            return "Success";
        }

        //Create the poll
        // POLLMASTER = OwnerAdd+PollID
        // POLLMASTERINFO/POLLMASTERVOTES = Works like a unique numberID for the SC, each time we get a data, 
        // we will only got from the one that we want
        public static void CreatePoll(string OwnerAdd, string Question)
        {
            string PollInfo;
            string PollMasterInfo;
            string PollVotes;
            string PollID = "0";
            OwnerAdd = OwnerAdd + "/";   // OWNERADD = 0x1234/
            PollMasterInfo = string.Concat(OwnerAdd,PollID); //PollMasterINFO = 0x1234/0
            //Generate a PoolID
            Storage.Get(Storage.CurrentContext, PollInfo, PollMasterInfo){
            	while(PollInfo != null){
            		Random r = new Random();
					int rInt = r.Next(0, 1000);
					PollID =(string) rInt;
            	}
            	PollID = PollID + "/"; // PollId= 0/
            	PollInfo = string.Concat(string.Concat(OwnerAdd,PollID),Question); // PollInfo= 0x1234/0/Do you like code
            	Storage.Put(Storage.CurrentContext, PollInfo, PollMasterInfo ); // [PollMasterInfo:0x1234/0/Do you like code]
            	PollInfo =string.Concat(OwnerAdd,PollID); // PollInfo= 0x1234/0/
            	//For count, we will do lenght -1
            	PollVotes = PollInfo + "A/B"; // PollVotes= 0x1234/0/A/B
            	PollMasterVotes = PollMasterInfo+"votes";  // PollMasterVotes = 0x1234/0votes
            	Storage.Put(Storage.CurrentContest, PollVotes, PollMasterVotes); //[PollMasterVotes:0x1234/0/A/B]
            	return PollID;
            }

        }

        //Access to the poll
        public static string[] AccessPoll(string OwnerAdd, string PollID)
        {
            string PollInfo;
            string[] PollInfoA;
            string PollMasterInfo;
            OwnerAdd = OwnerAdd + "/";
            PollMasterInfo = string.Concat(OwnerAdd,PollID); 
            PollInfo=Encoding.ASCII.GetBytes(Storage.Get(Storage.CurrentContext, PollMasterInfo));
            
            return PollInfoA;
        }

        //Poll Voting
        public static void VotePoll(string OwnerAdd, string PollID, string VoteOption)
        {
            string PollVotes;
            string[] PollVotesA;
            string PollMasterVotes;
            OwnerAdd = OwnerAdd + "/";	//    OWNERADD = '0x1234/'
            PollID = PollID + "votes"; // PollID = 0votes
            PollMasterVotes = string.Concat(OwnerAdd,PollID); //  POLLMASTERVOTES='0x1234/0votes'
            PollVotes=Encoding.ASCII.GetBytes(Storage.Get(Storage.CurrentContext, PollMasterVotes ));  // POLLVOTES='0x1234/0/AAA/BB'
            PollVotesA = PollVotes.Split("/"); //POLLVOTESA = ['0X1234','0','AAA','BB']
            //PollVotesA[0]=OwnerAdd
            //PollVotesA[1]=PollID
            //PollVotesA[2]=VotesA 
            //PollVotesA[3]=VotesB
            OwnerAdd = PollVotesA[0];
            OwnerAdd = OwnerAdd + "/";
            PollID = PollVotesA[1];
            PollID = PollID + "/";
            VotesA = PollVotesA[2];
            VotesB = PollVotesA[3];

            if(VoteOption =="A"){
                VotesA = VotesA+"A";
                VotesA = VotesA+"/";
                PollVotes = string.Concat(VotesA,VotesB);
                PollVotes = string.Concat(string.Concat(OwnerAdd,PollID),PollVotes);
                Storage.Put(Storage.CurrentContext, PollVotes, PollMasterVotes );

            }
            if(VoteOption =="B"){
                VotesB = VotesB+"B";
                VotesA= VotesA+"/"
                PollVotes = string.Concat(VotesA,VotesB);
                PollVotes = string.Concat(string.Concat(OwnerAdd,PollID),PollVotes);
                Storage.Put(Storage.CurrentContext, PollVotes,PollMasterVotes );
            }
            //IF the VoteOption is != of A|B we dont need to call Storage.Put
            
        }
        //Get Results of the Poll
        public static string[] ResultsPoll(string OwnerAdd, string PollID)
        {
            string PollResults;
            string[] PollResultsA = new string[3];
            string PollMasterVotes;
            OwnerAdd = OwnerAdd + "/";
            PollMasterVotes = string.Concat(OwnerAdd,PollID);
            PollResults=Encoding.ASCII.GetBytes(Storage.Get(Storage.CurrentContext, PollMasterVotes));
            PollResultsA = PollResults.Split("/");
            //PollResultsA[0]=OwnerAdd
            //PollResultsA[1]=PollID
            //PollResultsA[2]=VotesA 
            //PollResultsA[3]=VotesB
            return PollResultsA;
        }

    }

}