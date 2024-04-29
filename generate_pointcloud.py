import random

def foo(t:float) -> tuple:
    return (
        1.5*t*random.gauss(1.0,0.01), 
        0.7*t*random.gauss(1.0,0.01) + 0.2, 
        0.3*t*random.gauss(1.0,0.01) - 0.1, 
        -0.6*t*random.gauss(1.0,0.01) + 0.3
    )

def bar(t1:float, t2:float) -> tuple:
    return (
        1.5*t1*random.gauss(1.0,0.01), 
        0.7*t1*random.gauss(1.0,0.01) + 0.2, 
        0.3*t1*random.gauss(1.0,0.01) - 0.1, 
        -0.6*t2*random.gauss(1.0,0.01) + 0.3
    )

MIN_T = -2
MAX_T = 2
COUNT = 200
OUTPUT_PATH = "pointcloudplane.styl"
# Generate some data
data : list = list()
for i in range(COUNT):
    T1:float = random.uniform(MIN_T,MAX_T)
    T2:float = random.uniform(MIN_T,MAX_T)
    data.append(bar(T1,T2))

toWrite : str = "object.data\n"
# Convert the data to a string
for i in range(0,COUNT,4):
    toWrite += "[\n"
    for j in range(4):
        toWrite += f"({data[i+j][0]},{data[i+j][1]},{data[i+j][2]},{data[i+j][3]})\n"
    toWrite += "]\n"

# Write the data to the file
with(open(OUTPUT_PATH,"w") as file):
    file.write(toWrite)
    
