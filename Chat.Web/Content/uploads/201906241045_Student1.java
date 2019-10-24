/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package UDP;

import java.io.Serializable;

/**
 *
 * @author TaDuyHieu
 */
public class Student1 implements Serializable{
    private static final long serialVersionUID = 20151107;

    private String maSV;
    private String ten;
    private float tienganh;
    private float toan;
    private float tin;
    private boolean pass;

    public Student1(String maSV, String ten, float tienganh, float toan, float tin) {
        this.maSV = maSV;
        this.ten = ten;
        this.tienganh = tienganh;
        this.toan = toan;
        this.tin = tin;
    }

    public static long getSerialVersionUID() {
        return serialVersionUID;
    }

    public String getMaSV() {
        return maSV;
    }

    public String getTen() {
        return ten;
    }

    public float getTienganh() {
        return tienganh;
    }

    public float getToan() {
        return toan;
    }

    public float getTin() {
        return tin;
    }

    public boolean isPass() {
        return pass;
    }

    public void setMaSV(String maSV) {
        this.maSV = maSV;
    }

    public void setTen(String ten) {
        this.ten = ten;
    }

    public void setTienganh(float tienganh) {
        this.tienganh = tienganh;
    }

    public void setToan(float toan) {
        this.toan = toan;
    }

    public void setTin(float tin) {
        this.tin = tin;
    }

    public void setPass(boolean pass) {
        this.pass = pass;
    }
}
