import logging
import azure.functions as func
import json

def main(entry: str, result: func.Out[str]):
    logging.info(entry)
    data = json.loads(entry)
    logging.info("Stream entry from key 'streamTest2' with Id '" + str(data['Id']) + "' and values '" + str(data['Values']) + "'")
    result.set("streamTest2 " + str(data['Id']))