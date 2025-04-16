from flask import Flask, request, jsonify
from PIL import Image
import pytesseract
import io
import shutil
import platform
import os
from GPT import explain_question

# 自动检测 tesseract 可执行路径
tesseract_path = shutil.which("tesseract")

# Fallback 手动判断常见路径
if not tesseract_path:
    if platform.system() == "Darwin":
        if os.path.exists("/opt/homebrew/bin/tesseract"):
            tesseract_path = "/opt/homebrew/bin/tesseract"
        elif os.path.exists("/usr/local/bin/tesseract"):
            tesseract_path = "/usr/local/bin/tesseract"
    elif platform.system() == "Windows":
        tesseract_path = "C:\\Program Files\\Tesseract-OCR\\tesseract.exe"
    elif platform.system() == "Linux":
        tesseract_path = "/usr/bin/tesseract"

pytesseract.pytesseract.tesseract_cmd = tesseract_path
print(f"✅ 当前使用 tesseract 路径: {tesseract_path}")
app = Flask(__name__)

@app.route("/ocr", methods=["POST"])
def ocr_endpoint():
    if 'image' not in request.files:
        return "No image file found", 400

    file = request.files['image']
    image = Image.open(file.stream).convert("RGB")

    # 使用 pytesseract 进行 OCR 识别
    result = pytesseract.image_to_string(image, lang='chi_sim+eng')

    print("📄 OCR结果:", result.strip())
    return result.strip()

try:
    from paddleocr import PaddleOCR
    ocr_model = PaddleOCR(use_angle_cls=True, lang='ch')
except ImportError:
    ocr_model = None
    print("⚠️ PaddleOCR 未安装，/ocr_paddle 接口将无法使用")

@app.route("/ocr_paddle", methods=["POST"])
def ocr_paddle_endpoint():
    if ocr_model is None:
        return jsonify({"error": "PaddleOCR not available"}), 500

    if 'image' not in request.files:
        return jsonify({"error": "No image file found"}), 400

    file = request.files['image']
    image = Image.open(file.stream).convert("RGB")
    img_path = "temp_ocr.jpg"
    image.save(img_path)

    result = ocr_model.ocr(img_path, cls=True)
    lines_with_boxes = [
        {
            "text": line[1][0],
            "box": line[0]
        }
        for line in result[0]
    ]
    os.remove(img_path)

    print("📄 PaddleOCR结构化结果:", lines_with_boxes)
    return jsonify({"results": lines_with_boxes})

@app.route("/gpt_explain", methods=["POST"])
def gpt_explain():
    print("📥 收到 GPT 接口请求", flush=True)
    data = request.get_json()
    question = data.get("question", "")

    if not question:
        return jsonify({"error": "No question provided"}), 400

    try:
        explanation = explain_question(question)
        print("🧠 GPT 解释结果:", explanation)
        return jsonify({"explanation": explanation})
    except Exception as e:
        return jsonify({"error": str(e)}), 500

@app.route("/gpt_explain_step", methods=["POST"])
def gpt_explain_step():
    print("📥 收到 GPT 解题步骤请求", flush=True)
    data = request.get_json()
    question = data.get("question", "")

    if not question:
        return jsonify({"error": "No question provided"}), 400

    try:
        explanation = explain_question(question, prompt_mode="step")
        print("📘 解题步骤：", explanation)
        return jsonify({"explanation": explanation})
    except Exception as e:
        return jsonify({"error": str(e)}), 500

if __name__ == "__main__": 
    app.run(host="0.0.0.0", port=5001)