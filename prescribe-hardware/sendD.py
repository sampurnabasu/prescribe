import urllib       
import json  
import time     
import datetime        
import serial                                                        
from timeout import timeout    

@timeout(2)        #timeout if http problem  
def callApi():      #function to get information from firebase server          
    #print 'test1'                                       
    time.sleep(.2)           
    urlStr = "https://prescribe-94c86.firebaseio.com/.json" #link for my firebase server                       
    try:
        print 'try'
        #proxies = {'http': '189.112.3.87:3128','http' : '208.254.241.13:8080'} #using proxy     
        fileObj = urllib.urlopen(urlStr, proxies=None)  #get string content from the website       
        #print 'test2'                                   
        for line in fileObj: #iterate through lines in the website                     
            #print line 
            print line                                                            
            parsed_json = json.loads(line) #parse the string in json format                                                                                                      
            return parsed_json['pillCode']                                                                              
    except:
        print 'calling'
        return ""

def main():
    ser = serial.Serial('/dev/cu.usbmodem14141',19200)
    #ser = serial.Serial('/dev/ttyMFD1', 19200)
    #ser = serial.Serial('/dev/ttyGS0', 19200)
    #wait until connected
    print 'connected'
    lastResult = ""

    while 1:
        try:   #try catch block for debugging                          
            time.sleep(.95)                                      
            result=callApi()  #get the information from firebase server   

            if result=="": #failed GET call earlier
                print 'not connected'   
                continue                       
            print "good"                    #print good to know the we got the data from server successfully
            if(result != lastResult):
                ser.write(chr(int(result)))
                print "send:",chr(int(result))
                lastResult = result
        except:    #check for exception
            print 'waiting'
            #print "bad"
        time.sleep(1)                                                 

if __name__ == "__main__":
    main()