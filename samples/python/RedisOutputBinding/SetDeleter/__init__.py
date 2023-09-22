import logging
import azure.functions as func

def main(key: str):
    logging.info("Deleting recently SET key '" + key + "'")
    return [key]