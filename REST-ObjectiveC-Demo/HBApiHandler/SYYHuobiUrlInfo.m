//
//  SYYHuobiUrlInfo.m
//
//  Created by 似云悠 on 2017/12/13.
//  Copyright © 2017年 syy. All rights reserved.
//

#import "SYYHuobiUrlInfo.h"
#import "SYYEncryptionUtils.h"
#import "SYYUtils.h"
#import "SYYHuobiConstant.h"
#import "YYCategories.h"
#import <openssl/ecdsa.h>
#import <openssl/pem.h>

@implementation SYYHuobiUrlInfo

+ (SYYHuobiUrlInfo *)getUrlInfoWithRequeseMethod:(NSString *)requestMethod
                                            host:(NSString *)host
                                          path:(NSString *)path
                                          params:(NSDictionary *)params{
    //需要签名(需要拼接在url上的参数)
    NSMutableDictionary *signatureParamsDic = [self requiredParams];
    //不需要签名(在body上的参数)
       NSMutableDictionary *noSignatureParamsDic = [[NSMutableDictionary alloc] init];
    
    if([requestMethod isEqualToString:@"GET"]){
        [signatureParamsDic setValuesForKeysWithDictionary:params];
    }else if([requestMethod isEqualToString:@"POST"]){
        [noSignatureParamsDic setValuesForKeysWithDictionary:params];
    }
    
    NSString *signatureVaule = [self signatureWithRequeseMethod:requestMethod host:host path:path params:signatureParamsDic];
    
    
    [signatureParamsDic setValue:signatureVaule forKey:@"Signature"];
    
    NSString *url = [NSString stringWithFormat:@"%@%@",host,path];
    
    SYYHuobiUrlInfo *urlInfo = [[SYYHuobiUrlInfo alloc] init];
    urlInfo.url = [SYYUtils getGetUrlWithUrl:url params:signatureParamsDic];
    urlInfo.parames = noSignatureParamsDic;
    
    return urlInfo;
    
}


+ (NSString *)signatureWithRequeseMethod:(NSString *)requestMethod
                                    host:(NSString *)host
                                  path:(NSString *)path
                                  params:(NSDictionary *)params{
    
    
    host = [host stringByReplacingOccurrencesOfString:@"https://" withString:@""];
    host = [host stringByReplacingOccurrencesOfString:@"http://" withString:@""];
    
    NSString *signature = @"";
    //加密前先将值进行url转码
    params = [SYYUtils encodeParameterWithDictionary:params];
    //需要加密的参数名称（ascii排过序的）
    NSArray *paramsSortArray = [SYYUtils ASCIISortedArray:params.allKeys];
    NSMutableString *signatureOriginalStr = [[NSMutableString alloc] initWithString:@""];
    [signatureOriginalStr appendFormat:@"%@\n",requestMethod];
    [signatureOriginalStr appendFormat:@"%@\n",host];
    [signatureOriginalStr appendFormat:@"%@\n",path];
    
    for(int i=0; i<paramsSortArray.count; i++){
        NSString *key = paramsSortArray[i];
        if(i == 0){
            [signatureOriginalStr appendFormat:@"%@=%@",key,params[key]];
        }else{
            [signatureOriginalStr appendFormat:@"&%@=%@",key,params[key]];
        }
    }
    
    signature = [SYYEncryptionUtils hmac:signatureOriginalStr withKey:kHBSecretKey];
    
    return signature;
}

#pragma mark - Private

+ (NSString *)privateSignatureFromSign:(NSString *)sign {
    NSData *signData = [[sign dataUsingEncoding:NSUTF8StringEncoding] sha256Data];
    unsigned char *digest = (unsigned char *)signData.bytes;
    NSString *pivateKey = kHBPrivKey;
    const char *pemPrivKey = [pivateKey UTF8String];
    BIO *buf = BIO_new_mem_buf((void*)pemPrivKey, (int)pivateKey.length);
    EC_KEY *ecKey = PEM_read_bio_ECPrivateKey(buf, nil, nil, nil);
    int signDataLength = (int)signData.length;
    unsigned int signLen;
    unsigned char *signature = (unsigned char *)malloc(ECDSA_size(ecKey));
    ECDSA_sign(0, digest, signDataLength, signature, &signLen, ecKey);
    NSData *data =  [[NSData alloc] initWithBytes:signature length:signLen];
    NSString *privSign = [data base64EncodedString];
    return privSign;
}

/**
 签名必选参数
 */
+ (NSMutableDictionary *)requiredParams{
    
    NSMutableDictionary *params = @{@"AccessKeyId":
                                        kHBAccessKey,
                                    @"SignatureMethod":
                                        @"HmacSHA256",
                                    @"SignatureVersion":
                                        @"2",
                                    @"Timestamp":[SYYUtils dateTransformToTimeString:@"yyyy-MM-dd'T'HH:mm:ss"]
                                    }.mutableCopy;
    return params;
}

@end
