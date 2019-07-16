const fs = require('fs')
const csv = require('csvtojson')
const path = 'assets/2019q1/'

const convertToJson = (fileName) => {
    try {
        const csvFilePath=`${path}${fileName}.txt`
        console.log(csvFilePath)

        csv({delimiter:'\t'})
        .fromFile(csvFilePath)
        .then((jsonObj)=>{
            const jsonStr = JSON.stringify(jsonObj)
            fs.writeFileSync(`${path}${fileName}.json`, jsonStr, {encoding: 'utf8'})
        })
    } catch(error) {
        console.error(error);
    }
}

['tag', 'num_min', 'pre', 'sub'].forEach(convertToJson)