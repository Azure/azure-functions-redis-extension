import logging

def main(key: str) -> str:
    logging.info("Deleting recently SET key '" + key + "'")
    return key