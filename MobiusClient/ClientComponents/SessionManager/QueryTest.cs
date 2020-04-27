using Mobius.ComOps;
using Mobius.Data;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Management;
using System.Threading;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Run test query scripts
	/// </summary>

	public class QueryTest
	{
		static string TestResultsFolder; // folder containing test results
		static LogFile LogFile; // logfile for test results
		static string CurrentQueryName = "";

		public QueryTest()
		{
			return;
		}

        /// <summary>
        /// Run a query test script
        /// Example:
        ///  Run Test Queries "C:\TestQueryScript.txt" c:\TestResults

        /// </summary>
        /// <param name="commandLine"></param>
        /// <returns></returns>

        public static string Run(
			string args)
		{
			try
			{
				List<string> argList = Lex.ParseAllExcludingDelimiters(args);
				if (argList.Count != 2) return "Syntax: Run Test Queries <scriptFileName> <outputFolder>";

				string scriptFileName = Lex.RemoveAllQuotes(argList[0]);
				if (!File.Exists(scriptFileName)) throw new Exception("File not found: " + scriptFileName);

				SS.I.QueryTestMode = true;

				int queriesWithErrors = 0;
				bool qwecf = false; // queriesWithErrorCounterFlag if any error counted for current query
				int hitCountDifference = 0;
				int dataDifference = 0;

				TestResultsFolder = Lex.RemoveAllQuotes(argList[1]).TrimEnd('\\');
				if (!Directory.Exists(TestResultsFolder)) Directory.CreateDirectory(TestResultsFolder);

				string ds = DateTimeMx.GetCurrentDateTimeToMinuteResolution();
				string logFileName = TestResultsFolder + @"\TestResults - " + ds + ".log";
				LogFile = new LogFile(logFileName);
				LogFile.ResetFile();

				StreamReader sr = new StreamReader(scriptFileName);
				LogFile.Message("Running Test Query Script " + scriptFileName);

				while (true)
				{
					//if (Client.CancelRequested()) break;

					string line = sr.ReadLine();
					if (line == null) break;
					if (line.Trim() == "") continue;
					if (line.Trim().StartsWith(";")) continue; // comment
					string[] sa = line.Split(',');
					CurrentQueryName = sa[0].Trim();
					int expectedHitCount = Int32.Parse(sa[1].Trim());
					LogFile.Message("");
					LogFile.Message("/*** Query: " + CurrentQueryName + " ***/");

					try // catch any exception for the query in this code and continue to next query
					{
						qwecf = false;

						//clear out the "holder" for the test query SQL
						//--postponed since this gets pretty hairy
						//SS.I.QueryTestSQL="";

						Query newQuery = QbUtil.ReadQuery(CurrentQueryName);
						if (newQuery == null)
						{
							LogFile.Message("ERROR!!!: Query not found");
							queriesWithErrors++;
							continue;
						}
						QbUtil.OpenQuery(CurrentQueryName);
						if (QbUtil.Query == null || QbUtil.Query.Tables.Count == 0)
						{
							LogFile.Message("ERROR!!!: Query failed to open properly");
							queriesWithErrors++;
							continue;
						}

#if false // Special MQL conversion test
					string mql = MqlUtil.ConvertQueryToMql(QueryBuilder.Query); // to mql
					Query q = MqlUtil.ConvertMqlToQuery(mql); // back to query
					QueryBuilder.SetCurrentQueryInstance(q);
					QueryBuilder.RenderQuery(q,0);
#endif // End of MQL conversion test

						int time = TimeOfDay.Milliseconds();
						string response = "";
						try
						{
							response = QueryExec.RunQuery(QbUtil.Query, OutputDest.WinForms);
						}
						catch (Exception ex)
						{
							LogFile.Message("ERROR!!!: Exception: " + DebugLog.FormatExceptionMessage(ex));
							queriesWithErrors++;
							continue;
						}

						int hitCount = SessionManager.CurrentResultKeysCount; // save the hit count

						if (hitCount > 0)
						{
							QueryManager qm = SessionManager.Instance.QueryResultsQueryManager;
							Query q = qm.Query;
							DataTableManager dtm = qm.DataTableManager;

							while (true)
							{ // loop til we have some data in the DataTable
								if (qm.DataTable.Rows.Count > 0 ||
								 dtm.RowRetrievalState != RowRetrievalState.Running)
									break;

								Thread.Sleep(500);
								Application.DoEvents();
							}


							ExportCsvForResultsComparison(qm);
						}

						time = TimeOfDay.Milliseconds() - time;
						LogFile.Message("Execution Time(s): " + ((time + 500) / 1000).ToString() + ", hits: " + hitCount);

						QbUtil.CloseFile(false); // close the query file, noprompt

						if (response != "" && !response.ToLower().StartsWith("command") &&
							!response.ToLower().StartsWith("no data"))
						{
							LogFile.Message("ERROR!!!: RunQuery Error = " + response);
							queriesWithErrors++;
							continue;
						}
						else
						{
							// Check the data for discrepancies

							// -- hitCount first 
#if false // (disabled, difficult to keep up to date and gets caught in data checks anyway)
							if (hitCount != expectedHitCount)
							{
								LogFile.Message("ERROR!!!: Hit count difference, expected: " + expectedHitCount);

								CountError(ref queriesWithErrors, ref qwecf, ref hitCountDifference);
								//continue;
							}
#endif
							// (create the reference data directory if necessary)


							if (!Directory.Exists(TestResultsFolder + @"\Reference"))
							{
								Directory.CreateDirectory(TestResultsFolder + @"\Reference");
							}

							string csvFilename = newQuery.UserObject.Name + "_" +
												 newQuery.UserObject.Id + ".csv";
							LogFile.Message("Query-specific server-side datafile: " + csvFilename);
							string[] arrFilenames = new string[2];
							arrFilenames[0] = TestResultsFolder + @"\Reference\" + csvFilename;
							arrFilenames[1] = TestResultsFolder + @"\" + csvFilename;
							//handle the special cases when hit count is zero (and, therefore, no CSV file was created)
							if (hitCount == 0)
							{
								if (File.Exists(arrFilenames[0]))
								{
									LogFile.Message("WARNING: " + response);
									LogFile.Message("         Data comparison cannot be performed, but this query appears to have returned data in the past.  (See " + arrFilenames[0] + " on the server.)");
								}
								else
								{
									LogFile.Message("INFO: " + response);
									LogFile.Message("      No data were expected.");
								}
								continue;
							}
							//assume that the copy of the data for the current query was created
							//use it as the reference data if no reference data exists
							if (!File.Exists(arrFilenames[0]))
							{
								File.Copy(arrFilenames[1], arrFilenames[0], true);
								LogFile.Message("WARNING:  No reference data existed for the current version of query \"" + newQuery.UserObject.Name + "\".  The current data set has been copied to \"" + arrFilenames[0] + "\".");
								continue;
							}

							//compare the reference file and the latest run
							// assume that the files are equivalent until we have evidence to the contrary
							bool noDifferencesToReport = true;

							//assume that the two files have already been sorted such that all rows for each compound id are together
							TextReader[] TextReaders = new TextReader[2];
							TextReaders[0] = null;
							TextReaders[1] = null;
							try
							{
								TextReaders[0] = File.OpenText(arrFilenames[0]); //the reference
								TextReaders[1] = File.OpenText(arrFilenames[1]); //the current run

								// keep vars of discrepancies for report
								ArrayList metaDiscrepancies = new ArrayList();
								ArrayList labelDiscrepancies = new ArrayList();
								ArrayList[] dataRowDiscrepancies = { new ArrayList(), new ArrayList(), new ArrayList() }; //lost,gained,changed
																																																					//create data structs to keep lists of obselete/new SNs and details of data changes
																																																					// keep lists of SNs of interest
																																																					//  -- the flag indicating that there's a discrepancy to report (noDifferencesToReport) will be set to false
																																																					//     based on the SN set counts after all of the data has been compared
																																																					//  -- the error count will be incremented by the sum of the counts in 0,1,3
																																																					//  0: present in ref-new; 1: present in new-ref; 2: common to both but not equivalent
								ArrayList[] snSetsOfInterest = { new ArrayList(), new ArrayList(), new ArrayList() };
								// var to hold the counts of mt.mc differences
								ArrayList discrepancyCountSummary = new ArrayList();


								//Grab the important info from the first two rows of the ref and new files (metatable, metacolumn, and column header info)
								string[] MetaInfo = new string[2];
								string[] ColumnLabels = new string[2];
								//metadata is provided such that the reorder is simple/easy, namely a line of CSV composed of
								// 0. Metatable name
								// 1. Metacolumn name
								// 2. Index of the column in the report (column merging is taken into account)
								// delimited by '\0'.
								//Split at the ',' and sort the metainfo by "0 and-then 1 and-then 2" -- "conveniently", a string sort accomplishes this.
								//Assume that no metacolumns map into the same CSV column (NO merging!!!)
								ArrayList[] MetaInfo_RN = new ArrayList[2];
								for (int i = 0; i < 2; i++)
								{
									//used to alter column ordering as-needed
									MetaInfo[i] = TextReaders[i].ReadLine();
									MetaInfo_RN[i] = SplitCSVString(MetaInfo[i], ',');
									MetaInfo_RN[i].Sort(); //sorts mtName|mcName|reportColIdx strings
																				 //used for reporting purposes
									ColumnLabels[i] = TextReaders[i].ReadLine();
								}

								//for now, call the queries incomparable if the meta info isn't
								// an exact match in metatable and metacolumn names (independent of column reordering)
								//get data to report the lines that are new and old
								// start by getting the lists of metatable.metacolumn
								// (assume that if the same column appears twice in one and once in the other, then that's a legitimate difference)
								ArrayList[] mtmcLists = { new ArrayList(), new ArrayList() };
								for (int i = 0; i < 2; i++)
								{
									for (int j = 0; j < MetaInfo_RN[i].Count; j++)
									{
										string[] metaSplit = Lex.RemoveAllQuotes(((string)MetaInfo_RN[i][j])).Split(new char[] { '\0' });
										//we'll need this info again if the data sets are comparable, so retain these values
										MetaInfo_RN[i][j] = metaSplit;
                                        //add the current mt.mc to the list
                                        //Summarized data must compared separately from unsummarized data. nwr 3/1/2019 
                                        //if (Lex.EndsWith(metaSplit[0], MetaTable.SummarySuffix)) // fix for 2.2 -> 2.3 comparison
										//  metaSplit[0] = metaSplit[0].Substring(0, metaSplit[0].Length - 8);

										mtmcLists[i].Add(metaSplit[0].ToUpper() + "." + metaSplit[1].ToUpper());
									}
								}
								// eliminate duplicates -- this isn't horribly efficient, but it's easy to follow and the lists of columns should be very short...
								for (int i = mtmcLists[0].Count - 1; i >= 0; i--)
								{
									for (int j = mtmcLists[1].Count - 1; j >= 0; j--)
									{
										//if the same mt.mc appears in both lists, cross it off
										if (mtmcLists[1][j].Equals(mtmcLists[0][i]))
										{
											mtmcLists[0].RemoveAt(i);
											mtmcLists[1].RemoveAt(j);
											break;
										}
									}
								}
								//check for any exclusive mt.mc's
								if (mtmcLists[0].Count > 0 || mtmcLists[1].Count > 0)
								{
									noDifferencesToReport = false;
									metaDiscrepancies.Add("ERROR: The new data file does not appear to have the same set of source columns as the reference.  Data comparison will not be performed for this query.");

									//note the differences -- we could have both new and lost mtmc's, so allow both to be reported
									if (mtmcLists[0].Count > 0)
									{
										CountError(ref queriesWithErrors, ref qwecf, ref dataDifference);
										metaDiscrepancies.Add("\tColumns missing from the new file: " + JoinArrayList(mtmcLists[0], ", "));
									}
									if (mtmcLists[1].Count > 0)
									{
										CountError(ref queriesWithErrors, ref qwecf, ref dataDifference);
										metaDiscrepancies.Add("\tExtra columns in the new file: " + JoinArrayList(mtmcLists[1], ", "));
									}

									//all done with these two array lists
									mtmcLists = null;
								}
								else
								{
									//all done with these two array lists
									mtmcLists = null;

									//to possibly shortcut comparisons for very similar data sets, we'll note if the columns in the ref and new
									// data sets are in the same order
									bool sameColOrder = true;

									//since we need to look at columns grouped by metatable, we need to know which columns should be grouped.
									//we already have a sorted list of mt|mc|idx so use it get the desired info.
									//so that we can keep track of which data is where and not overly complicate future changes,
									//create separate variables to hold different kinds of data. (rather than building one large tree)
									ArrayList MetaTableNames = new ArrayList();
									ArrayList[] MetaColumnNames;  // one arraylist per metatable
									ArrayList[,] ColumnIndices_RN;   // ref/new and one arraylist per metatable
									ArrayList[,] ColumnLabels_RN;    // ref/new and one arraylist per metatable
																									 //get the list of distinct metatable names
									for (int i = 0; i < MetaInfo_RN[0].Count; i++)
									{
										string metaTableName = ((string[])MetaInfo_RN[0][i])[0];
                                        //Summarized data must compared separately from unsummarized data. nwr 3/1/2019 
                                        //if (Lex.EndsWith(metaTableName, MetaTable.SummarySuffix)) // fix for 2.2 -> 2.3 comparison
										//	metaTableName = metaTableName.Substring(0, metaTableName.Length - 8);
										if (!MetaTableNames.Contains(metaTableName))
										{
											MetaTableNames.Add(metaTableName);
										}
									}
									//by metatable, get the meta column names and indices for the ref and new CSV data
									MetaColumnNames = new ArrayList[MetaTableNames.Count];
									ColumnIndices_RN = new ArrayList[2, MetaTableNames.Count];
									for (int i = 0; i < MetaTableNames.Count; i++)
									{
										MetaColumnNames[i] = new ArrayList();
										ColumnIndices_RN[0, i] = new ArrayList();
										ColumnIndices_RN[1, i] = new ArrayList();
									}
									//this is rather inefficient, but the code's clean and fairly intelligible
									int[] arrColIdx = new int[2];
									for (int i = 0; i < MetaTableNames.Count; i++)
									{
										for (int j = 0; j < MetaInfo_RN[0].Count; j++) //recall that arrArlMetaInfo[0][i] == arrArlMetaInfo[1][i] and [0].Count == [1].Count
										{
											string[] refMetaSplit = (string[])(MetaInfo_RN[0][j]);
											if (refMetaSplit[0].Equals(MetaTableNames[i]))
											{
												MetaColumnNames[i].Add(refMetaSplit[1]);
												arrColIdx[0] = Convert.ToInt32(refMetaSplit[2]);
												ColumnIndices_RN[0, i].Add(arrColIdx[0]);

												string[] newMetaSplit = (string[])(MetaInfo_RN[1][j]);
												arrColIdx[1] = Convert.ToInt32(newMetaSplit[2]);
												ColumnIndices_RN[1, i].Add(arrColIdx[1]);

												if (arrColIdx[0] != arrColIdx[1])
												{
													sameColOrder = false;
												}
											}
										}
									}
									//now get the ref and new column labels (using the indices we've just grouped)
									ColumnLabels_RN = new ArrayList[2, MetaTableNames.Count];
									for (int i = 0; i < MetaTableNames.Count; i++)
									{
										ColumnLabels_RN[0, i] = new ArrayList();
										ColumnLabels_RN[1, i] = new ArrayList();
									}
									//split the ref and new strings of column labels into collections (ie, [2] -> [2][x])
									ArrayList[] arrArlColumnLabelSplits = new ArrayList[2];
									arrArlColumnLabelSplits[0] = SplitCSVString(ColumnLabels[0], ',');
									arrArlColumnLabelSplits[1] = SplitCSVString(ColumnLabels[1], ',');
									//group the column labels such that corresponding metacolumns/column indices/column labels are accessed via the same set of indices into the arrArl's
									for (int i = 0; i < MetaTableNames.Count; i++)
									{
										for (int j = 0; j < ColumnIndices_RN[0, i].Count; j++) // this is equivalent to [0,0].Count since the array is rectangular...
										{
											ColumnLabels_RN[0, i].Add(Lex.RemoveDoubleQuotes((string)(arrArlColumnLabelSplits[0][(int)ColumnIndices_RN[0, i][j]])));
											ColumnLabels_RN[1, i].Add(Lex.RemoveDoubleQuotes((string)(arrArlColumnLabelSplits[1][(int)ColumnIndices_RN[1, i][j]])));
										}
									}

									//we've extracted everything of interest from arrArlMetaInfo, arrArlColumnLabelSplits, and arrColumnLabels so null them out
									MetaInfo_RN = null;
									arrArlColumnLabelSplits = null;
									ColumnLabels = null;

									//note any changes in column labels (taking the reorder into account)
									for (int i = 0; i <= ColumnIndices_RN.GetUpperBound(1); i++) //by metatable
									{
										for (int j = 0; j < ColumnIndices_RN[0, i].Count; j++) //for each column (in post-mt.mc sort order)
										{
											//compare the column labels
											if (!ColumnLabels_RN[0, i][j].Equals(ColumnLabels_RN[1, i][j]))
											{
												//note the column positions so that this info can be reported
												int refColIdx = (int)ColumnIndices_RN[0, i][j];
												int newColIdx = (int)ColumnIndices_RN[1, i][j];
												labelDiscrepancies.Add("The label for column " + (refColIdx + 1) +
													" (\"" + ColumnLabels_RN[0, i][j] + "\") has changed to \"" + ColumnLabels_RN[1, i][j] + "\"" +
													((refColIdx == newColIdx) ? "" : " (column " + (newColIdx + 1) + " in the new result set.)"));
											}
										}
									}

									//now that we know how to reorganize it, we can start actually parsing through the data and finding differences

									//create the arrays to tally mt and mt.mc change counts
									// keep a count of where differences occur
									// -- populated after the number of metatables is known and the columns are verified to be comparable
									int[] tableDiscrepancyCount = new int[MetaTableNames.Count];
									int[][] columnDiscrepancyCount = new int[MetaTableNames.Count][];
									for (int i = 0; i < MetaTableNames.Count; i++)
									{
										columnDiscrepancyCount[i] = new int[MetaColumnNames[i].Count];
									}

									//get the blocks of data for common SNs and compare them
									// exclusive SNs are noted by the 
									ArrayList[] dataBlocks = { new ArrayList(), new ArrayList() }; //ref and new
									string[] currentSNs = { null, null }; //used to maintain context while retrieving data blocks
									string[] nextSNs = { null, null }; //used to maintain context while retrieving data blocks
									string[] nextLines = { null, null }; //used to maintain context while retrieving data blocks
									bool identicalDataBlocks;
									while (GetNextCommonSNBlocksFromTR(TextReaders, snSetsOfInterest, dataBlocks, currentSNs, nextLines, nextSNs))
									{
										//compare the blocks of data
										// if the column order is the same from ref to new, then there's at least a chance that the data blocks are identical
										// even if not, they may still be equivalent since the order of rows in the different subqueries can vary
										identicalDataBlocks = sameColOrder;

										//try to shortcut if the column ordering is the same
										if (sameColOrder && dataBlocks[0].Count == dataBlocks[1].Count)
										{
											for (int i = 0; i < dataBlocks[0].Count; i++)
											{
												if (!dataBlocks[0][i].Equals(dataBlocks[1][i]))
												{
													//the data blocks could still be equivalent.  we'll need to work a bit harder to compare the data blocks
													identicalDataBlocks = false;
													break;
												}
											}
										}
										else
										{
											identicalDataBlocks = false;
										}

										if (!identicalDataBlocks)
										{
											//check for equivalence
											// -- This could be done more compactly and efficiently, but, after a few minutes
											// -- on that path, I tried for something that might be intelligible instead.
											// -- Thank goodness that this code only executes as-needed on a per-substance basis.

											//For sanity's sake, should split the data reshape and empty and common row removal (1-5) off into a helper method...
											// 1. split the rows of csv data into component fields
											for (int i = 0; i < 2; i++)
											{
												for (int j = 0; j < dataBlocks[i].Count; j++)
												{
													dataBlocks[i][j] = SplitCSVString((string)dataBlocks[i][j], ',');
												}
											}
											// 2. rejoin fields in metatable-based groupings (reordering columns as desired at the same time)
											//    dataBlocks now references a array of arraylists of arraylists; cells are [ref/new][row][col]
											//    we want a re-indexing that (while rejoining subsets of cols into strings for sorting purposes)
											//    provides [ref/new][mt][row] in an array of arrays of arraylists
											ArrayList[,] mtDataBlocks = new ArrayList[2, MetaTableNames.Count];
											for (int i = 0; i < 2; i++)
											{
												for (int j = 0; j < MetaTableNames.Count; j++)
												{
													mtDataBlocks[i, j] = new ArrayList();
												}
											}
											ArrayList mtDataRow = new ArrayList(); //!!!What?!?
											for (int i = 0; i < 2; i++) //for ref and new
											{
												for (int j = 0; j < dataBlocks[i].Count; j++) //look at one row of the data block
												{
													for (int k = 0; k < MetaTableNames.Count; k++) //focus on ONE metatable at a time
													{
														//grab the data for columns from this metatable
														for (int m = 0; m < MetaColumnNames[k].Count; m++)
														{
															int colIdx = (int)ColumnIndices_RN[i, k][m];
															try { mtDataRow.Add(((ArrayList)dataBlocks[i][j])[colIdx]); }
															catch (Exception ex) { continue; }
														}

														//add this metatable's row to its portion of the block
														mtDataBlocks[i, k].Add(JoinArrayList(mtDataRow, ","));

														//clean the slate for next time
														mtDataRow.Clear(); //change this...
													}
													// 3. within each mtBlock, sort the rows -- skip this...  There's no point in sorting since the remaining steps distrust the row order.
													//		mtDataBlocks[i,k].Sort();
												}
											}

											//we now have a reshaped copy of the dataBlock data, so we could get rid of the previous copy
											//as long as we DO NOT destroy the ArrayList[] and its ArrayLists...  We'll use them to get the next block of data,
											//and they will be cleared then.
											dataBlocks[0].Clear();
											dataBlocks[1].Clear();

											// 4. within groupings, collapse to eliminate the empty rows
											for (int i = 0; i < 2; i++) //ref and new
											{
												for (int j = 0; j <= mtDataBlocks.GetUpperBound(1); j++) //each metatable
												{
													for (int k = mtDataBlocks[i, j].Count - 1; k >= 0; k--) //go through the rows in reverse order and delete "empty" rows
													{
														bool isEmpty = true;
														mtDataRow = SplitCSVString((string)(mtDataBlocks[i, j][k]), ',');
														for (int m = 0; m < mtDataRow.Count; m++)
														{
															if (!"".Equals(Lex.RemoveAllQuotes((string)mtDataRow[m]).Trim()))
															{
																isEmpty = false;
																break;
															}
														}
														if (isEmpty)
														{
															mtDataBlocks[i, j].RemoveAt(k);
														}
													}
												}
											}

											// 5. within groupings, drop rows that are identical (new-vs-ref)
											//    the number of rows for a given compound id number should be very small, so
											//    we can reasonably perform the n-times-m comparisons comparing ref to new
											//    we could handle this differently if the blocks were sorted, but sorting
											//    and efficiency are traded here for (relative) simplicity
											for (int i = 0; i <= mtDataBlocks.GetUpperBound(1); i++) //for each metatable
											{
												//ref or new could be empty for a given metatable...
												//if this is the case, then there are, obviously, no duplicates to eliminate.
												if (mtDataBlocks[0, i].Count != 0 && mtDataBlocks[1, i].Count != 0)
												{
													for (int j = mtDataBlocks[0, i].Count - 1; j >= 0; j--)
													{
														for (int k = mtDataBlocks[1, i].Count - 1; k >= 0; k--)
														{
															if (mtDataBlocks[0, i][j].Equals(mtDataBlocks[1, i][k]))
															{
																mtDataBlocks[0, i].RemoveAt(j);
																mtDataBlocks[1, i].RemoveAt(k);
																break;
															}
														}
													}
												}
											}

											// 6. within groupings, if there's not data left then the data was equivalent
											bool equivalentSNData = true;
											for (int i = 0; i <= mtDataBlocks.GetUpperBound(1); i++) //for each metatable
											{
												if (mtDataBlocks[0, i].Count > 0 || mtDataBlocks[1, i].Count > 0)
												{
													equivalentSNData = false;
													break;
												}
											}

											//if the data isn't equivalent, then we need to determine exactly how it differs
											// differences will ALL be reported, but error count will only be incremented by one for the set of all changes for the SN
											if (!equivalentSNData)
											{
												//if the data for the current SN isn't equivalent, then we get to add it to the list of changed SNs
												snSetsOfInterest[2].Add((string)currentSNs[0]);

												//For each metatable,
												for (int i = 0; i <= mtDataBlocks.GetUpperBound(1); i++)
												{
													if (mtDataBlocks[0, i].Count > 0 || mtDataBlocks[1, i].Count > 0)
													{
														//split the rows into component fields for easier comparison
														// (we'll need to rejoin for reporting)
														for (int j = 0; j < 2; j++)
														{
															for (int k = 0; k < mtDataBlocks[j, i].Count; k++)
															{
																mtDataBlocks[j, i][k] = SplitCSVString((string)(mtDataBlocks[j, i][k]), ',');
															}
														}

														//If both still have data, then compare ref to new rows to guess which rows correspond
														// do this first so that the corresponding rows can be reported and eliminated
														// before gained/lost rows are reported.  (important if we have 3 new and 2 ref or vice versa)
														if (mtDataBlocks[0, i].Count > 0 && mtDataBlocks[1, i].Count > 0)
														{
															// 7. compare rows pair-wise (one new, one ref) to decide which rows *probably* correspond to one another
															//this is horrific...  it's roughly O(n-cubed) in terms of row count
															//oh well...  Get the counts of common values for the rows comparing ref and new
															int[,] matchingColValCounts = new int[mtDataBlocks[0, i].Count, mtDataBlocks[1, i].Count]; // ref-rows x new-rows matrix
															for (int j = 0; j < mtDataBlocks[0, i].Count; j++) //outer loop of ref rows
															{
																for (int k = 0; k < mtDataBlocks[1, i].Count; k++) //middle loop of new rows
																{
																	for (int m = 0; m < MetaColumnNames[i].Count; m++) // inner loop of columns -- same number of cols in ref and new
																	{
																		if (((ArrayList)mtDataBlocks[0, i][j])[m].Equals(((ArrayList)mtDataBlocks[1, i][k])[m]))
																		{
																			matchingColValCounts[j, k]++;
																		}
																	}
																}
															}
															//find the most similar rows and consider them to be related if they have at least one common field value
															ArrayList[] matchedRows = { new ArrayList(), new ArrayList() }; //use a pair of arraylists because we'll need to sort to eliminate rows
															int maxCommonFieldCount;
															ArrayList commonFieldCount = new ArrayList();
															int[] mostSimRows;
															do
															{
																maxCommonFieldCount = 0;
																mostSimRows = new int[2];
																for (int j = 0; j < mtDataBlocks[0, i].Count; j++) //outer loop of ref rows
																{
																	for (int k = 0; k < mtDataBlocks[1, i].Count; k++) //inner loop of new rows
																	{
																		if (matchingColValCounts[j, k] > maxCommonFieldCount)
																		{
																			maxCommonFieldCount = matchingColValCounts[j, k];
																			mostSimRows[0] = j;
																			mostSimRows[1] = k;
																		}
																	}
																}
																if (maxCommonFieldCount > 0)
																{
																	matchedRows[0].Add(mostSimRows[0]);
																	matchedRows[1].Add(mostSimRows[1]);
																	commonFieldCount.Add(maxCommonFieldCount);
																	//now that this pair of rows has been matched, they cannot be matched with any others
																	//set the match count to -1 to indicate that they've already been used
																	for (int j = 0; j < mtDataBlocks[1, i].Count; j++) //don't match this ref row with other new rows
																	{
																		matchingColValCounts[mostSimRows[0], j] = -1;
																	}
																	for (int j = 0; j < mtDataBlocks[0, i].Count; j++) //don't match this new row with other ref rows
																	{
																		matchingColValCounts[j, mostSimRows[1]] = -1;
																	}
																}
															} while (maxCommonFieldCount > 0);
															// 8. Note the in-row differences (new-vs-ref data and increment mt and mt.mc change counts so that freq by column name and index in the new csv file, metatable name, and metacolumn name can be reported for the query in total)
															// We'll report ref/new mt rows to provide some context for understanding the differences
															if (matchedRows[0].Count > 0)
															{
																//add mc info for the changed rows
																dataRowDiscrepancies[2].Add("Data changed for substance \"" + currentSNs[0] + "\" in metatable " + (i + 1) +
																	" (\"" + MetaTableNames[i] + "\"):");
																dataRowDiscrepancies[2].Add("\t MC: " + JoinArrayList(MetaColumnNames[i], ", "));

																for (int j = 0; j < matchedRows[0].Count; j++)
																{
																	int[] rowPair = { (int)(matchedRows[0][j]), (int)(matchedRows[1][j]) };

																	//tally column-level differences
																	int colCount = MetaColumnNames[i].Count;
																	for (int k = 0; k < colCount; k++)
																	{
																		if (!((ArrayList)mtDataBlocks[0, i][rowPair[0]])[k].Equals(((ArrayList)mtDataBlocks[1, i][rowPair[1]])[k]))
																		{
																			columnDiscrepancyCount[i][k]++;
																		}
																	}

																	//prep report of changed data
																	tableDiscrepancyCount[i]++;
																	dataRowDiscrepancies[2].Add("\tREF " + (j + 1) + ": " + JoinArrayList((ArrayList)mtDataBlocks[0, i][rowPair[0]], ", "));
																	dataRowDiscrepancies[2].Add("\tNEW " + (j + 1) + ": " + JoinArrayList((ArrayList)mtDataBlocks[1, i][rowPair[1]], ", "));
																}
																//consume the matched rows
																for (int j = 0; j < 2; j++)
																{
																	matchedRows[j].Sort();
																	for (int k = matchedRows[j].Count - 1; k >= 0; k--) //remove from the bottom up to avoid problems with the index changing
																	{
																		mtDataBlocks[j, i].RemoveAt((int)matchedRows[j][k]);
																	}
																}
															}
														}

														//If data has been added or removed then handle that now
														if (mtDataBlocks[0, i].Count > 0)
														{
															//lost data
															dataRowDiscrepancies[0].Add("Data lost for substance \"" + currentSNs[0] + "\" from metatable " + (i + 1) +
																" (\"" + MetaTableNames[i] + "\"):");
															dataRowDiscrepancies[0].Add("\t MC: " + JoinArrayList(MetaColumnNames[i], ", "));
															for (int j = 0; j < mtDataBlocks[0, i].Count; j++)
															{
																tableDiscrepancyCount[i]++;
																dataRowDiscrepancies[0].Add("\tREF: " + JoinArrayList((ArrayList)mtDataBlocks[0, i][j], ", "));
															}
														}
														else if (mtDataBlocks[1, i].Count > 0)
														{
															//new data
															dataRowDiscrepancies[1].Add("New data for substance \"" + currentSNs[0] + "\" in metatable " + (i + 1) +
																" (\"" + MetaTableNames[i] + "\"):");
															dataRowDiscrepancies[1].Add("\t MC: " + JoinArrayList(MetaColumnNames[i], ", "));
															for (int j = 0; j < mtDataBlocks[1, i].Count; j++)
															{
																tableDiscrepancyCount[i]++;
																dataRowDiscrepancies[1].Add("\tNEW: " + JoinArrayList((ArrayList)mtDataBlocks[1, i][j], ", "));
															}
														}
													}
												} // end of loop over metatables
											} // end of detailed comparison of SN data
										} //end of column reordering/regrouping comparison
									} // end of sn-specific data block comparison

									for (int i = 0; i <= tableDiscrepancyCount.GetUpperBound(0); i++)
									{
										int tdc = tableDiscrepancyCount[i];
										if (tdc > 0)
										{
											discrepancyCountSummary.Add(MetaTableNames[i] + ": " + tdc);
											for (int j = 0; j <= columnDiscrepancyCount[i].GetUpperBound(0); j++)
											{
												int cdc = columnDiscrepancyCount[i][j];
												if (cdc > 0)
												{
													discrepancyCountSummary.Add("\t" + MetaColumnNames[i][j] + ": " + cdc);
												}
											}
										}
									}
								} // close of detailed file comparison (mt.mc sets were equivalent)

								//done finding deltas in this one
								TextReaders[1].Close();
								TextReaders[0].Close();

								//if we have any SN differences, then there are difference to report
								noDifferencesToReport = (noDifferencesToReport && snSetsOfInterest[0].Count == 0 && snSetsOfInterest[1].Count == 0 && snSetsOfInterest[2].Count == 0);

								//report any differences
								if (noDifferencesToReport)
								{
									LogFile.Message("INFO:  No meaningful differences for query \"" + newQuery.UserObject.Name + "\".");
								}
								else
								{
									if (metaDiscrepancies.Count > 0)
									{
										LogFile.Message(JoinArrayList(metaDiscrepancies, "\r\n"));
									}
									else
									{
										if (labelDiscrepancies.Count > 0)
										{
											LogFile.Message(JoinArrayList(labelDiscrepancies, "\r\n"));
										}

										CountError(ref queriesWithErrors, ref qwecf, ref dataDifference);
										dataDifference += snSetsOfInterest[0].Count + snSetsOfInterest[1].Count + snSetsOfInterest[2].Count - 1;

										LogFile.Message("ERROR:  Data differences were detected for query \"" + newQuery.UserObject.Name + "\".");

										//counts of new, obselete, unchanged, and changed SNs might be useful...
										//Include this info with the lists.  Total "hits" was already reported.

										//report the new and apparently obselete SNs
										if (snSetsOfInterest[1].Count > 0)
										{
											LogFile.Message("\tNew SNs (" + snSetsOfInterest[1].Count + "): " + JoinArrayList(snSetsOfInterest[1], ", "));
										}
										if (snSetsOfInterest[0].Count > 0)
										{
											LogFile.Message("\tLost SNs (" + snSetsOfInterest[0].Count + "): " + JoinArrayList(snSetsOfInterest[0], ", "));
										}

										//report SNs with data changes
										if (snSetsOfInterest[2].Count > 0)
										{
											LogFile.Message("\tSNs with data discrepancies (" + snSetsOfInterest[2].Count + "): " + JoinArrayList(snSetsOfInterest[2], ", "));
										}

										//report row-level difference counts grouped by metatable.metacolumn
										LogFile.Message("\tDiscrepancy Counts:");
										LogFile.Message("\t\t" + JoinArrayList(discrepancyCountSummary, "\r\n\t\t"));

										//report row-level data differences
										if (snSetsOfInterest[2].Count > 0)
										{
											LogFile.Message("\tDetails:");
											for (int i = 0; i < 3; i++)
											{
												if (dataRowDiscrepancies[i].Count > 0)
												{
													LogFile.Message("\t\t" + JoinArrayList(dataRowDiscrepancies[i], "\r\n\t\t"));
												}
											}
										}

									}
								}  //end of data comparison and reporting for current query
							}
							catch (Exception ex)
							{
								LogFile.Message("ERROR: Unexpected error while verifying that query was unchanged.");
								LogFile.Message(DebugLog.FormatExceptionMessage(ex));
								if (TextReaders[0] != null)
								{
									TextReaders[0].Close();
								}
								if (TextReaders[1] != null)
								{
									TextReaders[1].Close();
								}
								throw new Exception(ex.Message, ex);
							}
						}
					}

					// catch any exception for the query in this code and continue to next query

					catch (Exception ex) 
					{
						string msg2 = "/*** Unexpected error testing code for query: " + CurrentQueryName + " ***/\r\n";

						LogFile.Message(msg2 + DebugLog.FormatExceptionMessage(ex));
						continue; // continue to next query
					}

				} // query loop

				sr.Close();
				SS.I.QueryTestMode = false;

				GC.Collect(); // force garbage collection
				GC.WaitForPendingFinalizers();

				string msg =
					"Test Complete, Queries with Errors: " + queriesWithErrors + 
					//", Hit count diff: " + hitCountDifference + 
					", Data differences: " + dataDifference +
					"\r\nOutput file: " + logFileName;
				LogFile.Message("");
				LogFile.Message(msg);
				return msg;
			}
			catch (Exception ex)
			{
				SS.I.QueryTestMode = false;
				DebugLog.Message(ex);
				return ex.Message;
			}
		}

		/**
		 *	Helper method...  currentSN, nextLine, and nextSN can all be null
		 **/
		private static bool GetNextCommonSNBlocksFromTR(TextReader[] arrTextReaders, ArrayList[] snSetsOfInterest,
			ArrayList[] dataBlocks, string[] currentSNs, string[] nextLines, string[] nextSNs)
		{
			//flag to show that we've successfully retrieved a blocks of data from both ref and new for an SN
			bool successful = false;

			//discard the old data blocks
			dataBlocks[0].Clear();
			dataBlocks[1].Clear();

			//get fresh data blocks
			for (int i = 0; i < 2; i++)
			{
				GetSNBlockFromTR(arrTextReaders[i], dataBlocks[i], out currentSNs[i], ref nextLines[i], out nextSNs[i]);
			}

			if (currentSNs[0] != null && currentSNs[1] != null)
			{
				do
				{
					int snComparison = currentSNs[0].CompareTo(currentSNs[1]);
					if (snComparison == 0)
					{
						//we have two data blocks for the same SN, as desired
						successful = true;
					}
					else if (snComparison < 0)
					{
						//looks like we have an obselete SN (part of ref-new)
						snSetsOfInterest[0].Add(currentSNs[0]);
						GetSNBlockFromTR(arrTextReaders[0], dataBlocks[0], out currentSNs[0], ref nextLines[0], out nextSNs[0]);
					}
					else if (snComparison > 0)
					{
						//looks like we have a new SN (part of new-ref)
						snSetsOfInterest[1].Add(currentSNs[1]);
						GetSNBlockFromTR(arrTextReaders[1], dataBlocks[1], out currentSNs[1], ref nextLines[1], out nextSNs[1]);
					}
				}
				while (!successful && currentSNs[0] != null && currentSNs[1] != null);
			}

			//if one of the textreaders is out of data, then consume the rest of the other
			if (currentSNs[0] == null && currentSNs[1] != null)
			{
				//consume all trailing new SNs
				do
				{
					snSetsOfInterest[1].Add(currentSNs[1]);
					GetSNBlockFromTR(arrTextReaders[1], dataBlocks[1], out currentSNs[1], ref nextLines[1], out nextSNs[1]);
				} while (currentSNs[1] != null);
			}
			else if (currentSNs[1] == null && currentSNs[0] != null)
			{
				//consume all trailing obselete SNs
				do
				{
					snSetsOfInterest[0].Add(currentSNs[0]);
					GetSNBlockFromTR(arrTextReaders[0], dataBlocks[0], out currentSNs[0], ref nextLines[0], out nextSNs[0]);
				} while (currentSNs[0] != null);
			}

			return successful;
		}

		private static void GetSNBlockFromTR(TextReader tr, ArrayList lineArray,
			out string currentSN, ref string nextLine, out string nextSN)
		{
			//discard the old lines
			lineArray.Clear();
			//if null, get the firstLine and currentSN
			if (nextLine == null)
			{
				nextLine = tr.ReadLine();
			}
			//if data is available, put some into the array
			//otherwise return with the array empty
			if (nextLine == null)
			{
				currentSN = null;
				nextSN = null;
			}
			else
			{
				//note the current SN
				int idx = nextLine.IndexOf(",");
				if (idx == -1)
				{
					currentSN = nextLine;
				}
				else
				{
					currentSN = nextLine.Substring(0, idx);
				}
				//read in lines until we hit EOF or get the first row with a new SN
				do
				{
					//add the line since we know that it's for the current SN
					lineArray.Add(nextLine);
					//get another line
					nextLine = tr.ReadLine();
					if (nextLine == null)
					{
						nextSN = null;
					}
					else
					{
						idx = nextLine.IndexOf(",");
						if (idx == -1)
						{
							nextSN = nextLine;
						}
						else
						{
							nextSN = nextLine.Substring(0, idx);
						}
					}
				} while (currentSN.Equals(nextSN));
				//should have new values for everything of interest...
			}
		}

		/// <summary>
		/// JoinArrayList
		/// </summary>
		/// <param name="arr"></param>
		/// <param name="delimiter"></param>
		/// <returns></returns>
		private static string JoinArrayList(ArrayList arr, string delimiter)
		{
			string retVal = null;
			if (arr != null && arr.Count > 0)
			{
				delimiter = ((delimiter == null) ? "" : delimiter);
				StringBuilder sb = new StringBuilder();
				sb.Append(arr[0].ToString());
				for (int i = 1; i < arr.Count; i++)
				{
					sb.Append(delimiter);
					sb.Append(arr[i].ToString());
				}
				retVal = sb.ToString();
				if (retVal.Contains("\0")) retVal = retVal.Replace("\0", " "); // hack fixup
			}
			return (retVal);
		}

		/// <summary>
		/// SplitCSVString
		/// </summary>
		/// <param name="cvsLine"></param>
		/// <param name="delimiter"></param>
		/// <returns></returns>
		private static ArrayList SplitCSVString(string cvsLine, char delimiter)
		{
			ArrayList retVal = null;

			if (cvsLine != null)
			{
				//first split doesn't respect the atomic nature of quoted strings
				string[] firstSplit = cvsLine.Split(new char[] { delimiter });

				//the final split may include remerging some of the split cells
				ArrayList finalSplit = new ArrayList();
				bool inQuote = false;
				StringBuilder sb = new StringBuilder();
				char[] chars;
				for (int i = 0; i < firstSplit.Length; i++)
				{
					if (inQuote)
					{
						//merge the current element with the quoted value so far
						sb.Append(delimiter);
						sb.Append(firstSplit[i]);
						//determine if the quote has been terminated
						if (firstSplit[i].Length > 0)
						{
							chars = firstSplit[i].ToCharArray();
							for (int j = chars.Length - 1; j >= 0; j--)
							{
								//a tail of quotation marks would represent trailing quotes in the value of the field
								//if the count is odd, then the value is terminated.  If even, then not.
								if (chars[j] == '"')
								{
									inQuote = !inQuote;
								}
								else
								{
									break;
								}
							}
						}
					}
					else
					{
						sb.Append(firstSplit[i]);
						//determine if a quote has begun and, if so, whether or not it has been terminated
						if (firstSplit[i].Length > 0 && firstSplit[i].Substring(0, 1).Equals("\""))
						{
							inQuote = true; //because of the first character

							chars = firstSplit[i].ToCharArray();
							for (int j = chars.Length - 1; j > 0; j--) //exclude the first character since it's already been counted
							{
								//a tail of quotation marks would represent trailing quotes in the value of the field
								//if the count is odd, then the value is terminated.  If even, then not.
								if (chars[j] == '"')
								{
									inQuote = !inQuote;
								}
								else
								{
									break;
								}
							}
						}
					}
					//add the new field if it's ready
					if (!inQuote)
					{
						finalSplit.Add(sb.ToString());
						//clear so that we're ready to build the next
						sb.Length = 0;
					}
				}
				retVal = finalSplit;
			}
			return retVal;
		}


		/// <summary>
		/// JoinStringArray
		/// </summary>
		/// <param name="stringArray"></param>
		/// <param name="delimiter"></param>
		/// <returns>null if </returns>
		private static string JoinStringArray(string[] stringArray, string delimiter)
		{
			string retVal = null;
			if (stringArray != null && stringArray.Length > 0)
			{
				delimiter = ((delimiter == null) ? "" : delimiter);
				StringBuilder sb = new StringBuilder();
				sb.Append(stringArray[0]);
				for (int i = 1; i < stringArray.Length; i++)
				{
					sb.Append(delimiter);
					sb.Append(stringArray[i]);
				}
				retVal = sb.ToString();
			}
			return retVal;
		}

		/// <summary>
		/// Write out the a csv file containing query results for test comparison
		/// </summary>

		static void ExportCsvForResultsComparison(QueryManager parentQm)
		{
			QueryManager qm = SessionManager.Instance.QueryResultsQueryManager;

			Query q = qm.Query;
			DataTableManager dtm = qm.DataTableManager;

			ResultsFormat rf = new ResultsFormat(qm, OutputDest.TextFile);
			rf.ExportFileFormat = ExportFileFormat.Csv;
			rf.ExportStructureFormat = ExportStructureFormat.Smiles; // export structures as smiles since chime values vary

			rf.OutputFileName = TestResultsFolder + // name output file as <queryName>_<queryId>
				"\\" + qm.Query.UserObject.Name + "_" + qm.Query.UserObject.Id + ".csv";
			rf.QualifiedNumberSplit = QnfEnum.Combined;

			ResultsFormatter.ExecuteExport(qm, parentQm, false);
			if (!Lex.IsNullOrEmpty(rf.ErrorMessage))
				throw new Exception(rf.ErrorMessage);

			// Sort the output file (file could be large)
			// 1. read it in (the column headers are special)

			List<string> lines = new List<string>();
			string columnHeaders = "";
			if (File.Exists(rf.OutputFileName))
			{
				System.IO.TextReader tr = File.OpenText(rf.OutputFileName);
				string line = null;
				columnHeaders = line = tr.ReadLine();
				do
				{
					line = tr.ReadLine();
					if (line != null)
					{
						lines.Add(line);
					}
				} while (line != null);
				tr.Close();
			}

			// 2. sort it
			lines.Sort();

			// 3. spit it back out -- with header info

			System.IO.TextWriter tw = File.CreateText(rf.OutputFileName);

			//add metadata
			// a line of metatable/metacolumn/position info
			// -- don't add commas to the metatable and/or metacolumn names
			// -- add the positional information as a suffix using a character that will not
			//    appear in a metatable or metacolumn name and that won't alter the sort order undesirably
			// -- need the column identification info in a format that makes it easy to re-order
			//    the columns to undo any ordering that a user might have applied

			StringBuilder metaInfo = new StringBuilder();
			string mtName, mcName;
			int reportColIdx = -1;
			char separator = '\0';

			for (int i = 0; i < rf.Tables.Count; i++)
			{
				ResultsTable rt = rf.Tables[i];
				for (int j = 0; j < rt.Fields.Count; j++)
				{
					ResultsField rfld = rt.Fields[j];
					if (rfld.Merge)
					{
						throw new Exception("The test query data comparator doesn't currently support merged columns!");
					}
					reportColIdx++;
					mtName = rfld.MetaColumn.MetaTable.Name;
					mcName = rfld.MetaColumn.Name;
					//could check here that the names don't include the separator, but it seems pointless...
					if (i == 0 && j == 0)
					{
						metaInfo.Append(Lex.Dq(mtName + separator + mcName + separator + reportColIdx));
					}
					else
					{
						//delimit with a comma prefix
						metaInfo.Append(",");
						metaInfo.Append(Lex.Dq(mtName + separator + mcName + separator + reportColIdx));
					}
				}
			}
			tw.WriteLine(metaInfo.ToString());

			//add header info
			tw.WriteLine(columnHeaders);

			//add data
			for (int i = 0; i < lines.Count; i++)
			{
				tw.WriteLine(lines[i]);
			}
			tw.Close();

			return;
		}

		static void CountError(ref int counter1, ref bool counted1, ref int counter2)
		{
			if (!counted1)
			{
				counter1++;
				counted1 = true;
			}

			counter2++;
		}


		public static void LogMessage(string msg)
		{
			LogFile.Message(msg);
		}


	}
}
