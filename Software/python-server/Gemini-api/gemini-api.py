import google.generativeai as genai

genai.configure(api_key="AIzaSyAFpwjnWz8SbWA-1yPdl-iDqAI7rQVUnjc")
genai_model = genai.GenerativeModel("gemini-1.5-pro")

def gemini(text):
    response = genai_model.generate_content(text)
    return response.text

text = '你是一个小学老师，请用少于100个字向上小学的小朋友科普玫瑰花,用："小朋友你好，这是玫瑰花"开头'
respond = gemini(text)
print(respond)