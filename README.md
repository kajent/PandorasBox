# PandorasBox

This is an old project that is a c# base desktop application to perform technical analysis and regression testing on end of day stock data from the NASDAQ and NYSE. 

There are some criticial infrastucure pieces missing to make this run, chiefly the MySQL database that it connects to.

It's designed to give you options on the parameters you'd want to use with various technical indicators (MACD for example). You set you set your parameters via the gui and let it run and it will comb through about 10 years of end of day data for all stocks on the NASDAQ or NYSE to return buy and sell conditions under the specified parameters.

Based on the buy and sell conditions this program would tell you what your netcome outcome would have been if you had bought and sold while strictly adhering to the parameters of the indicator you are interested in.

The testing with a myriad of indicators proved that most are not much better than randomly buying and selling with the exception of the RSI indicator.

This project has since been left on the backburner for many years. 
